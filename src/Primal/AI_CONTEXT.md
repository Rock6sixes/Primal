# Primal (Rain World mod) — context for picking this back up

Read this whole file before touching code. It exists because this project has been through two
major architecture pivots and several misdiagnosed bugs, and re-deriving that history from the
code alone wastes time. Written 2026-09-11.

AFTER READING THIS, PROMPT USER ABOUT WHETHER SAINT NEEDS THE FIRST CYCLE GIMMICK IGNORE - AFTER USER ANSWERS, TURN THAT CHECKBOX ON OR OFF AND REMOVE THIS COMMENT. (Rivulet permanent shelter failure: implemented 2026-11-05, see below.)

## Open items (as of 2026-11-10)

Built but **not yet tested in-game**:
- Rivulet shelter routing: Sleep In Every Shelter requirement + Permanent Shelter Failure gimmick
  (2026-11-05). Checklist in its own section below.
- Precycle Timer rework (2026-11-10): head start in seconds (0-480, default 60), Random length
  checkbox, 100% chance, 100% flood, on for Rivulet by default.
- Chains rescan fix (2026-11-10): untracked creatures in a tracked room get picked up within half a
  second. See "Chains: creatures left unchained" below.

Waiting on a user decision:
- Saint: should No Hold skip the first cycle (header note above)?
- Chains escape timer: defaults to 90 s; when it fires, every creature still alive in the room is
  left unchained for the rest of that visit. Intended, or should the default be 0 (no escape)?
- Survivor's 50 community-challenge requirements are still not defaulted onto Survivor.
- No design yet for Watcher (and any character missing from CHARACTER_DESIGN_STATUS.md).
- TimeLimit game-over screen: deliberately deferred by the user.

Older items this doc still lists as untested (from Sept. 2026 -- may since have been checked by
the user; confirm and prune): Threat Slowdown, NoPassages, Inv roster entry, gimmick-assignment
persistence fix, multiple Primal slots (rounds 4-5), Remix gimmick-picker scrolling, Primal
character names.

Housekeeping: the claude.ai project's copy of this file (`claude/AI_CONTEXT.md`) is an old
snapshot; this repo copy is authoritative.

## Chains: creatures left unchained (2026-11-10)

Reported: one chainable creature sometimes stays unchained, mostly when few creatures are around
(e.g. only an overseer, room counted as cleared), possibly after it entered the room.

Two causes found by code review:
1. **Missed residents (bug, fixed).** Tracking started only from the room-entry scan
   (`GetCreatureList`, skips any creature `inShortcut` at that moment) and the `Creature.NewRoom`
   arrival hook. Vanilla calls `NewRoom` only when a creature exits a shortcut into a DIFFERENT
   room (`Creature.SpitOutOfShortCut`: `if (newRoom != room)`). A creature riding one of the room's
   own in-room shortcuts when you walked in was skipped by the scan and then came back out with no
   `NewRoom` -- never tracked. Creatures placed straight into a room (`PlaceInRoom`) never call
   `NewRoom` either. Fix: `RescanResidents` re-runs the entry scan every 20 ticks and feeds any
   untracked eligible creature to `TryAddArrivingCreature` (locks the room if it was empty/cleared).
   Gated on CatchLateArrivals (default on), skipped while the player is mid-shortcut, and suppressed
   for the rest of a visit after the escape timer released the room (`ChainState.Escaped`). Logs
   `[Chains] ... rescan found untracked X in ROOM -- adding`.
2. **Escape timer (by design, unchanged).** Default 90 s; when it fires the room unlocks with
   creatures still alive and unchained. The log line now says how many were left. Ask the user
   whether that matches what they saw.

Overseers are chainable by default (their ban checkbox defaults off).



## Precycle Timer: head start in seconds, 100% chance, on for Rivulet (2026-11-10, later)

Explicit request, supersedes parts of the section below:
- **Length is now set in seconds**, not percent of vanilla: `<char>_PrecycleTimer_headStartSeconds`
  (new config key, so old percent values are never misread), range 0-480, **default 60 (1 minute)**.
  Default = exactly that; "Random precycle length" rolls between it and 2.5x it (vanilla's
  4800:12000 ratio, so 120 s + Random = vanilla's 2-5 min); "Force max" = 2.5x it (max 20 min).
  The seconds box no longer greys out under Force max. Old `_durationPercent` key is unused.
- **0 seconds** = no head start: the Postfix zeroes `maxPreTimer`/`preTimer` and returns before
  rain pulses/flood, i.e. exactly vanilla's own no-precycle state (every vanilla reader of
  `maxPreTimer` already guards 0 -- checked RainCycle, ShelterDoor, ElectricDeath, AbstractCreature).
- **Frequency default 100%** (was 2%).
- **Precycle Timer is now defaulted onto Rivulet** (`GimmickDefaults.cs`). Rivulet's default
  campaign: 1-minute precycle every cycle (floods), then Rain Timer 0% (~1 s), Permanent Shelter
  Failure, Sleep In Every Shelter, story ending.
- Builds clean; not yet tested in-game.

## Precycle Timer default changes (2026-11-10)

Explicit request. Applies to every character the gimmick is assigned to (these are the gimmick's
own defaults, not Rivulet-only):
- **Head start defaults to the minimum, not a random roll.** Vanilla rolls
  `Random.Range(4800f, 12000f)` ticks (2-5 minutes, `RainCycle.cs` lines 293/318). New
  per-character setting `<char>_PrecycleTimer_randomDuration` ("Random precycle length", default
  off): off = always the scaled minimum (`4800 * durationPercent / 100`, i.e. 2 minutes at 100%);
  on = vanilla-style roll between the scaled bounds. "Force max precycle timer" still overrides both
  (and greys the new checkbox out). New checkbox row sits at `topY - 204` in the settings UI.
- **Flood chance defaults to 100%** (was 2%), so every triggered precycle floods unless turned down.
- Frequency is still 2% by default, and Precycle Timer is still not assigned to Rivulet by default.
- Remix keeps saved values: a character whose Precycle settings were already saved keeps them; the
  new defaults only apply to settings never saved.
- Builds clean; not yet tested in-game.

## Rivulet shelter routing: Sleep In Every Shelter + Permanent Shelter Failure (2026-11-05)

**Design (user):** Primal Rivulet must sleep in every shelter, and every shelter slept in breaks for
the rest of the campaign -- "a game of routing to get every shelter without being able to go back."
Built in a cloud session from the GitHub repo; compiles clean (`dotnet build --source <empty dir>`
on Linux, since nuget.org is blocked there and the project needs no packages). **Not yet tested
in-game.**

- `Gimmicks/PermanentShelterFailureGimmick.cs` (new):
  - `ShelterSleepRecords` -- shared per-(character, shelter room) sleep markers on
    DeathPersistentSaveData (`PrimalShelterSlept`), recorded from
    `SurvivorEvents.OnSuccessfulHibernation` (Win Prefix, so it lands in the save Win writes). Only
    recorded while the gimmick or the requirement is assigned to that character. Also snapshots
    vanilla's own `brokenShelters` per World right after load (timeline-broken shelters only).
  - `PermanentShelterFailureGimmick` (ID `PermanentShelterFailure`) -- Postfix on
    `World.LoadWorld(Timeline, ...)` and `World.LoadWorldForFastTravel`: forces
    `brokenShelters[i] = true` for every shelter with a sleep marker. Fast-travel World has
    `game == null`, so it reads `rainWorld.progression.currentSaveState` instead. `Register()` is
    wired into `RainWorldHooks.Postfix` after `RequirementRegistry.RegisterAll()`.
  - Vanilla side effects of a broken shelter: you wake in the (now broken) shelter you slept in;
    used shelters drop their icon on the HUD map; Room lets rain into a broken shelter if its
    DangerType allows; passages can't target a used shelter; running out of reachable shelters
    ends the run.
- `Gimmicks/Requirements.cs` -> `SleepEveryShelterRequirement` (ID `SleepEveryShelter`, "Sleep In
  Every Shelter"): same scope as VisitShelter (World.shelters minus DisabledMapRooms, every
  SlugcatStoryRegions region) but counts only sleeps, and skips timeline-broken shelters.
  Region-complete marker `PrimalShelterSleptRegionComplete`; logs `n/m shelters slept` per sleep.
- Defaults: `GimmickDefaults.cs` adds PermanentShelterFailure to Rivulet; `RequirementDefaults.cs`
  swaps Rivulet's VisitShelter default for SleepEveryShelter (VisitShelter still registered). An
  existing saved Remix config keeps whatever was assigned before.
- **Untested risk (Gotcha #1):** reads `World.brokenShelters`/`shelters`/`DisabledMapRooms`
  directly. These look public, and existing code (VisitShelter, PrecycleForceShelterUnavailableHook)
  already does the same, but neither has been confirmed against the real assembly. A
  `FieldAccessException` in `BepInEx/LogOutput.log` would mean switching to AccessTools.
- **Test checklist:** on a fresh Primal Rivulet save, sleep once -> log shows `[ShelterSleepRecords]
  ... slept in X`; next cycle the door of X doesn't close and log shows `[PermanentShelterFailure]
  ... broken: X`; X's map icon is gone; X stays broken after a death and after a game restart; a
  passage can't target X; `[SleepEveryShelter] n/m` count climbs per region.

## Mod renamed: AltCharacters -> Primal (2026-09-12)

This mod was originally built and shipped under the name **AltCharacters** (namespace
`AltCharacters`, assembly `AltCharacters.dll`, plugin/config GUID `rokas.altcharacters`, project
folder `src/AltCharacters/`). At the user's request it was fully rebranded to **Primal** on
2026-09-12 — namespace, assembly name, project folder, every class/file whose name derived from
"AltCharacters" or "AltChar" (`AltCharactersOptions` -> `PrimalOptions`, `AltSlot` -> `PrimalSlot`,
`AltCharSlotOptionsMenuHooks` -> `PrimalSlotOptionsMenuHooks` and its four hook classes, the "ALT
CHAR SLOT" Options-menu button text -> "PRIMAL SLOT", the Remix screen title, `modinfo.json`'s
`name` field), and **the plugin GUID itself, `rokas.altcharacters` -> `rokas.primal`** (the user
explicitly chose this over keeping the old GUID, understanding it means Rain World treats this as a
fresh mod install and every previously-saved (character, gimmick) assignment in
`ModConfigs\rokas.altcharacters.txt` is orphaned — starts over at hardcoded defaults under the new
`ModConfigs\rokas.primal.txt`, same as the very first launch ever). `OneSpearGimmick.cs`'s
`SaveMarkerKey` ("ALTCHAR_ONESPEAR", used to count lifetime spear-pulls already spent per save file)
was deliberately left **unrenamed** despite the "ALTCHAR" prefix — unlike the config file, this key
is embedded inside actual save files, and renaming it would silently reset every save's pull count
to 0 for no real benefit (it's invisible internal bookkeeping, never shown in any UI).

This file (`AI_CONTEXT.md`) was renamed throughout to match — every current/forward-looking mention
of the old name, namespace, or paths now says "Primal" — **except** the handful of places that
quote an exact historical log or exception line verbatim (grep for `AltCharacters` if you need to
find them): those are left exactly as actually captured, since the whole point of quoting them is
that they're byte-for-byte reproducible from a real log file, and at the time they were captured
the mod really was still called AltCharacters. Don't "fix" those quotes to say Primal.

**2026-09-15: the leftover pre-rename `src/AltCharacters/` project tree deleted entirely.** The
rename above only ever touched `src/Primal/` — the old `src/AltCharacters/` folder (its own full
copy of the source at the time of the rename, its own build output, and its own now-superseded
duplicate of this exact file) had been left sitting untouched in the repo since 2026-09-12, not
excluded from anything, not referenced by any build, just dead weight that could easily be mistaken
for a second active project by a future session. Deleted outright (`git rm -r`) rather than
excluded — this repo is real git history now, so removing it loses nothing recoverable. If you're
looking for the pre-rename version of any of this doc's own history, it's in this file's own git
log, not a second file on disk.

## 2026-09-15 (later): sessions now run directly on the user's own Windows machine, not a cloud container

Every "Container environment notes" section and every "this session's container has no `.NET`
toolchain / no git / has to push files through `device_commit_files`" caveat elsewhere in this doc
describes a **different, no-longer-current** setup. As of this update, the working directory
(`C:\Users\rokas\Documents\RWMods`) **is** the user's real machine — a real git repo (not
"no git repo here" as the Build & deploy workflow section still claims), with a working `dotnet`
(confirmed: `dotnet --version` -> 8.0.425) and direct filesystem access to both the source tree and
the actual Steam game install. `scripts/build-deploy-run.sh` can now be run directly and actually
builds/deploys/launches for real, instead of being hand-reviewed-only and shipped through a
device-bridge as described throughout the rest of this file. Don't assume the container limitations
(no toolchain, no decompiler, file transfer via `SendUserFile`/`device_commit_files`) still apply —
check directly (`dotnet --version`, `git status`) before assuming either way, the same "verify,
don't assume" discipline Gotcha #5 already established for build staleness.

**First real build in this environment found the 2026-09-15 whole-codebase refactor didn't actually
compile.** `dotnet build` (via `scripts/build-deploy-run.sh`) failed with 9x `CS0103: The name
'MoreSlugcatsEnums' does not exist in the current context`, all in `Gimmicks/GimmickRegistry.cs`
(the new `homeCharacter:` arguments added that day — see "Gimmick buttons colored by their home
character" above). Root cause was mundane: that file was missing `using MoreSlugcats;` entirely (no
`using` directives at all at its top), unlike every sibling file (`GimmickDefaults.cs`, etc.) that
already has it. **Fixed** by adding the one `using` line. Rebuilt clean (0 errors), the
`CopyToModFolder` post-build step + the script's own deploy step copied the DLL into
`...\StreamingAssets\mods\Primal\plugins\Primal.dll`, and `./RainWorld.exe` launched successfully —
`BepInEx/LogOutput.log` shows a clean chainloader startup with no errors, and no `exceptionLog.txt`
was generated. This is the first confirmed-clean build+launch since the 2026-09-15 refactor; **it
does not mean any individual gimmick/feature listed as "not yet tested" elsewhere in this doc is now
verified** — only that the mod loads without immediately crashing BepInEx's own chainloader. Note
also: `BepInEx/LogOutput.log` shows `Primal` (along with every other optional local mod --
moreslugcats, watcher, jollycoop, rwremix, versioning) as **"Skipping loading [Primal] because it's
not enabled"** — this is the in-game Mods-menu enabled/disabled list, unrelated to the build/deploy
fix above, and needs to be toggled on from Rain World's own mod-manager screen before any gimmick
can actually be exercised in-game.

## Maintenance policy — keep this document current

**This file is a living document, not a one-time snapshot.** Whoever (whichever session) is
working on this mod should update it as you go, not just when explicitly asked to summarize:

- After any architecture decision, pivot, or discarded approach — add it to History (or a new
  section if it doesn't fit) so it isn't re-litigated later.
- After root-causing any bug, especially a "silent"/hard-to-diagnose one — add it as a Gotcha, with
  the symptom, the actual cause, and how it was found (not just the fix), so the same investigation
  isn't repeated from scratch.
- After adding, removing, or substantially changing a file — update the file map.
- After the user reports a test result (confirmed working, still broken, wrong behavior) — update
  **Outstanding items** so the next session knows current status without re-reading the whole
  conversation history.
- Before a conversation is likely to end (context running low, task paused, user says they'll test
  and report back later) — do a pass over this whole file to make sure it still matches reality.

Keep it accurate over keeping it short: prune stuff that's no longer true rather than leaving stale
info, but don't hesitate to add detail future sessions will need. This instruction itself should
stay in the document — don't remove this section when editing.

## What this is, in plain terms

Rain World is a Unity game moddable via BepInEx + Harmony. This is a BepInEx plugin, C#, targeting
.NET Framework 4.8, built with `dotnet build` (MSBuild), using Harmony to patch the game's own
compiled methods at runtime (no source access to the game itself — only a decompiled/"publicized"
reference DLL to compile against, see **Gotcha #1** below, which matters a lot).

**The feature, from the player's perspective:** the Options menu gets a 4th save-slot button,
"PRIMAL SLOT", alongside vanilla's 3 normal save slots. Switching to it is a completely normal
save-slot switch (same vanilla code path, same busy-spinner, same failure/revert handling — this
mod does not reimplement any of that). On that one slot, and only on that slot, real vanilla
characters can have extra "gimmicks" layered on top of them — small gameplay modifiers like
"Gourmand gets exhausted much more easily" or "Spearmaster's iconic tail-spear-pull only works
once ever." Off that slot, every character plays 100% unmodified vanilla.

**Important: there are no custom characters.** Two earlier designs tried creating 9 new SlugBase
characters (clones of the vanilla roster) to carry the alt-mode identity. Both were scrapped by
explicit user decision — see **History** below. The current, correct design applies gimmicks
directly to the real vanilla `Player` object, gated on which save slot is mounted. If you ever
find yourself wanting to add a new SlugBase character or a "roster" concept back in: don't. That
was tried twice and rejected both times for good reasons (see History).

## Current architecture (as of this doc)

- `PrimalSlot.ReservedSlot = 666666` — the **first** of a range of save slot numbers this mod
  reserves: one per real vanilla slot, so real slot N's Primal counterpart is `ReservedSlot + N`
  (`PrimalSlotFor`/`RealSlotFor`). Chosen because Rain World only ever uses `saveSlot` to build a
  filename string, never as an index, so arbitrarily large numbers are safe, and nothing else will
  plausibly generate anything in this range.
- `PrimalSlot.IsActive(RainWorld)` — `rainWorld.options.saveSlot >= ReservedSlot`, i.e. a **range**
  check, not equality against one fixed slot (it was equality until the 2026-09-13 multiple-Primal-
  slots change; see PrimalSlot.cs's own header). This is the single gate every gimmick patch checks
  before doing anything.
- A gimmick is: one Harmony patch class + a stable string ID. It reads
  `player.playerState.slugcatCharacter.value` (the player's **real, unfaked** vanilla identity) and
  `GimmickAssignments.IsEnabled(characterID, gimmickID)`, and only then modifies behavior. In
  practice every patch does that through **`GimmickGate`** (see below) rather than open-coding it.
- `GimmickRegistry` lists the known gimmicks — as of 2026-09-18, 16: `Misery`, `Slugpup`, `OneSpear`
  (shown in the UI as "Master Spear"), `TimeLimit`, `NoHold` (shown as "No Hold" -- generalized
  2026-09-18 from the old single-purpose `NoSpears`, which no longer exists; see NoHoldGimmick.cs),
  `Chains`, `NoYeeks`, `NoCicadas`, `NoMauling`, `NoPassages`, `ThreatSlowdown`, `ThreatPanic`,
  `CreatureKillList` (shown as "Kill List"), `RainTimer` (new 2026-09-18; see RainTimerGimmick.cs),
  `BlizzardTimer` (new 2026-09-18; see BlizzardTimerGimmick.cs), `PrecycleTimer` (new 2026-09-18,
  later still; see PrecycleTimerGimmick.cs). Each `GimmickDef` takes its ID from
  the owning patch class's own `ID` const rather than re-typing
  the string, so the two can't drift apart silently. (Note: this mod also grew a large, separate
  "Requirements"/Community Challenges architecture in a later 2026-09-16/17 session, generalized
  onto its own Remix tab -- see "the Requirements architecture" and the Community Challenges
  sections much further down this doc; this bullet only tracks the original per-character
  GimmickRegistry list, which is still a live, separate system from Requirements.)
  `GimmickAssignments` is the runtime enabled/disabled lookup (Remix-configured override, falling
  back to a hardcoded default from `GimmickDefaults`). `PrimalOptions` builds the Remix menu
  screen as a **permanent character grid with a detail panel appended below it** — see "Config menu
  UI, in detail" below for the full design; the original flat one-checkbox-per-(character,gimmick)
  grid was replaced (because it stopped scaling once the gimmick count started climbing toward ~15)
  by a drill-down navigation UI, which was itself then replaced by the current merged-panel design
  (2026-09-11, at the user's explicit request — see that section for why).
- `GimmickDef` (see `Gimmicks/GimmickDef.cs`) optionally carries `BindSettings`/`BuildSettingsUI` —
  a gimmick can expose its own settings sub-view (e.g. TimeLimit's minutes + per-cycle/per-run
  toggle) beyond the plain enabled/disabled flag. See that file's own doc comments and the section
  below for the pattern. Gimmicks with no settings at all (both null, so the screen shows a plain
  "no settings" message instead): `Misery`, `NoYeeks`, `NoCicadas`, `NoMauling`, `NoPassages`,
  `CreatureKillList`.
- **Shared helpers (added 2026-09-15, in the refactor pass — see "Shared helpers and the 2026-09-15
  refactor, in detail" below).** Four small classes every gimmick now leans on instead of
  re-implementing the same thing per file:
  `GimmickGate` (the player → real character → IsActive → IsEnabled gate, in one place),
  `PrimalSaveMarkers` (this mod's `unrecognizedSaveStrings` marker format + a cached lookup),
  `PrimalMenuAccess` (the one reflected `KarmaLadderScreen.saveState` handle the menu-side patches
  share), and `ThreatReader` (the threat lookup both threat gimmicks read, memoized per frame).
  `PrimalSettings` holds the one mod-wide (not per-character) setting, the slot-switch key.
- `PrimalSlotOptionsMenuHooks.cs` adds the 4th button to the vanilla Options menu and makes it
  drive the exact same `Menu.OptionsMenu.SetCurrentlySelectedOfSeries("SaveSlot", ...)` /
  `GetCurrentlySelectedOfSeries` / `ProgressionBusy` / `HandleSaveSlotChangeFailed` machinery
  vanilla's own 3 save-slot buttons use, via two small Prefixes that translate our virtual button
  index (3) to/from the real reserved slot number (666666). It deliberately does not reimplement
  any slot-switching logic itself.
- `RainWorldHooks.cs` — a Postfix on `RainWorld.OnModsInit`. This is where `GimmickDefaults.Apply()`
  runs and where the Remix options screen gets registered (`MachineConnector.SetRegisteredOI`).
  **Do not move this earlier** — see Gotcha #2.
- `PrimalCharacterNames.cs` (new, 2026-09-11; revised 2026-09-15 twice) — while on the alt slot,
  relabels **every** character on the "choose your character" screen as "PRIMAL &lt;NAME&gt;".
  Gourmand, Spearmaster, Sofanthiel, and Artificer get "PRIMAL GOURMAND" / "PRIMAL SPEARMASTER" /
  "PRIMAL ???" / "PRIMAL ARTIFICER" with a written description listing whichever gimmicks are
  currently assigned to them (Sofanthiel has none). Survivor, Monk, Hunter, Rivulet, Saint, and
  Watcher (added in the second 2026-09-15 revision) get "PRIMAL SURVIVOR" / "PRIMAL MONK" /
  "PRIMAL HUNTER" / "PRIMAL RIVULET" / "PRIMAL SAINT" / "PRIMAL WATCHER" with a plain, static
  description: "Primal version not implemented". See "Primal character names, in detail" below.
- `PrimalUnimplementedPortraits.cs` (new, 2026-09-15) — companion to the above: on the alt slot,
  blacks out the character-select portrait for those same six "not implemented" characters, so the
  status reads visually as well as in text. A Postfix on `Menu.MenuDepthIllustration.GrafUpdate`
  that recolors the specific illustration whose filename contains "Slugcat" (the actual
  character-portrait layer, distinct from the background/vines/pipes/plants layers around it) to
  solid black, gated to the six target characters' own `MenuScene.SceneID`s. See "Primal character
  portraits, in detail" below for why it hooks `GrafUpdate` specifically rather than a one-time
  recolor.

### File map (`src/Primal/`)

- `Plugin.cs` — BepInEx entry point. `OnEnable()` just does `_harmony.PatchAll()`, wrapped in
  try/catch with logging. Deliberately touches nothing game-content-related (see Gotcha #2).
- `PrimalSlot.cs` — the reserved-slot base constant and the `IsActive` gate, described above.
  Rewritten 2026-09-13 (round 5, "multiple Primal slots"): the earlier `LastRealSlot` field (a
  single process-global "real slot to switch back to") is gone, replaced by a pure stateless
  mapping, `PrimalSlotFor(realSlot)` / `RealSlotFor(primalSlot)` (`ReservedSlot + realSlot` and its
  inverse), so real slot 0/1/2 each get their own independent Primal counterpart
  (666666/666667/666668) instead of all three sharing one. `IsActive` is now `saveSlot >=
  ReservedSlot` (a range check) rather than exact equality against one fixed number. See **Primal
  Slot Tab-switch, in detail** below.
- `Gimmicks/GimmickDef.cs` — data class: ID + display name + description, plus optional
  `BindSettings`/`BuildSettingsUI` hooks for gimmicks with their own settings sub-view (see below).
- `Gimmicks/GimmickRegistry.cs` — the list of all gimmicks (`GimmickRegistry.All`). Each entry's ID
  comes from the owning patch class's own `ID` const (`MiseryEffect.ID`, `ChainsGimmick.ID`, ...)
  rather than a re-typed string literal, so a typo can't silently unwire a gimmick.
- `Gimmicks/GimmickAssignments.cs` — runtime "is gimmick G enabled on character C" lookup
  (Remix-bound `Configurable<bool>`, falling back to a hardcoded default). Unchanged by the UI
  overhaul — `PrimalOptions` still calls `Register` for every (character, gimmick) pair
  exactly as before; only how those pairs are *displayed* changed. **2026-09-15:** the composite
  dictionary key is now an allocation-free `AssignmentKey` struct instead of `characterID + "|" +
  gimmickID` (which allocated a string on every call of a method the doc claimed didn't allocate,
  from the mod's hottest paths), and `GetConfigurable` was added so `PrimalOptions` can stop
  keeping a second parallel dictionary of the same objects under a duplicate key format.
- `Gimmicks/GimmickGate.cs` (new, 2026-09-15) — the shared "should this gimmick do anything for
  this player right now" gate: resolve the live `RainWorld` off the player, `PrimalSlot.IsActive`,
  read the real character ID, `GimmickAssignments.IsEnabled`. Replaces ~21 copy-pasted copies of
  that block and unifies the two different ways patches used to reach `RainWorld` onto the robust
  one (`abstractCreature.world.game`, valid mid-construction, with `room.game` as fallback). Also
  has `IsEnabledForCharacter` for the menu-side patches, which have no `Player` to read.
- `Gimmicks/PrimalSaveMarkers.cs` (new, 2026-09-15) — this mod's `DeathPersistentSaveData
  .unrecognizedSaveStrings` marker convention (the `<dpB>` delimiter) and a per-save-data cached
  lookup, shared by OneSpear, the Kill List, and the Slugpup ending flag. The cache is what makes
  the Slugpup flag safe to check from `Player.isSlugpup`'s getter; see the refactor section below.
- `Gimmicks/PrimalMenuAccess.cs` (new, 2026-09-15) — the single reflected
  `Menu.KarmaLadderScreen.saveState` handle (and the character ID derived from it) that
  `NoPassagesGimmick` and both Kill List menu hooks used to each cache separately.
- `Gimmicks/ThreatReader.cs` (new, 2026-09-15) — the threat lookup and the "threat level 1"
  threshold, extracted out of `ThreatSlowdownGimmick` (where `ThreatPanicGimmick` reached across to
  call them) and memoized per `Time.frameCount`, since both gimmicks Postfix the same method and
  Monk has both by default — so the whole reflection chain was running twice per tick.
- `Gimmicks/PrimalSettings.cs` (new, 2026-09-15) — mod-wide (not per-character) settings. Currently
  just the slot-switch key, previously a hardcoded `KeyCode.Tab` in `PrimalSlotTabSwitchHook`.
- `Gimmicks/GimmickDefaults.cs` — the mod's out-of-the-box gimmick assignments (currently: Gourmand
  gets Misery + Slugpup; Spearmaster gets OneSpear + TimeLimit + NoSpears; Artificer gets Chains).
  Called from `RainWorldHooks`, not `Plugin`.
- `Gimmicks/PrimalOptions.cs` — the Remix config screen. Source was changed (2026-09-11) to
  make the character-select grid permanent with a detail area appended below it (see "Config menu
  UI, in detail" below for the design). **Confirmed working in-game as of 2026-09-13** (see that
  section's own update note — the earlier "left alone, still behaves like the old drill-down"
  report has been superseded). Updated again 2026-09-13 to wrap the add-gimmick picker
  (`RenderAddGimmick`) in a scrollable `OpScrollBox`, since 12 registered gimmicks could no longer
  all fit/be reached in the picker's fixed vertical space — see "Add-gimmick picker made
  scrollable" under "Config menu UI, in detail". First attempt crashed immediately on open
  (`OpScrollBox` has to be added to the `OpTab` before its own `AddItems` can be called) — fixed
  same day by splitting the commit into two steps. Not yet rebuilt/retested since that fix.
- `Gimmicks/RainWorldHooks.cs` — runs `GimmickDefaults.Apply()` and registers the Remix screen,
  both on `RainWorld.OnModsInit` (see Gotcha #2 for why not earlier).
- `Gimmicks/PrimalSlotOptionsMenuHooks.cs` — the 4th "PRIMAL SLOT" Options-menu button. Updated
  2026-09-13 (round 5, alongside `PrimalSlotTabSwitchHook.cs` and `PrimalSlot.cs`, same request):
  `PrimalSlotSetSelectedHook.Prefix` now takes `__instance` and maps to whichever real slot is
  *currently mounted's own* Primal counterpart (`PrimalSlot.PrimalSlotFor`) instead of always
  targeting one fixed `ReservedSlot`; `PrimalSlotGetSelectedHook.Prefix` now checks
  `PrimalSlot.IsActive` (any Primal slot) instead of exact equality against one number. The button
  itself stays a single toggle — see **Primal Slot Tab-switch, in detail** below, "MULTIPLE PRIMAL
  SLOTS," for why a single button still makes sense with three underlying reserved slots.
- `Gimmicks/PrimalSlotTabSwitchHook.cs` (new, 2026-09-13; extended 2026-09-13 x5) — press Tab on
  the character-select screen to toggle between the current real save slot and that slot's own
  Primal counterpart, with a black loading-screen cover while `PlayerProgression` mounts, bookended
  by a vertical slide transition, and preserving whichever character was selected across the
  switch. See **Primal Slot Tab-switch, in detail** below. **Base switch and the round-3 transition
  (container-wide slide + cubic easing) both confirmed working in-game; round-4 (switching to the
  same character instead of whatever the other slot's save data defaults to) and round-5 (a
  separate Primal slot per real slot, instead of one shared by all three) both pushed together but
  not yet built/tested as of this doc.** 54,684 bytes as of round 5 (round 4 was 52,753). Touches
  more genuinely-new API surface (`PlayerProgression`'s ctor/`Destroy`,
  `ProcessManager.RequestMainProcessSwitch`, `Menu.container`, and `SlugcatSelectMenu`'s
  `slugcatPages`/`slugcatPageIndex`/`scroll`/`lastScroll`/`quedSideInput`) than anything else in
  this codebase; read Gotcha #1 again before touching it. Round 5 itself only changed how
  `targetSlot` is computed in `PrimalSlotTabSwitchHook.Postfix` (now `PrimalSlot.RealSlotFor`/
  `PrimalSlotFor` instead of reading/writing the now-removed `PrimalSlot.LastRealSlot`) — no new
  reflected surface, since everything downstream already treated `targetSlot` as an opaque int.
- `Gimmicks/PrimalCharacterNames.cs` — a Postfix on `Menu.SlugcatSelectMenu.SlugcatPageNewGame`'s
  constructor (the "choose your character, read the blurb" screen shown before a run exists on
  that character in that save slot) that relabels Gourmand/Spearmaster as "PRIMAL GOURMAND"/"PRIMAL
  SPEARMASTER" with a gimmick-driven description, but only while `PrimalSlot.IsActive` — every other
  character, and both of these off the alt slot, keep vanilla's own name/description untouched. See
  "Primal character names, in detail" below.
- `Gimmicks/MiseryGimmick.cs` — the Misery gimmick. Traced from a third-party mod, see **Misery
  gimmick, in detail** below — read this before touching it, the mechanic is non-obvious.
- `Gimmicks/SlugpupGimmick.cs` — seven patches/classes making up the Slugpup gimmick, **rewritten
  2026-09-15** onto a completely different mechanism than its original `SlugCatClass`-flip
  approach. `SlugpupGimmick` now forces `Player.isSlugpup`'s getter to return `true` (Postfix,
  `MethodType.Getter`) *without ever touching `SlugCatClass`* — the same trick the reference
  "Pupify" mod's own `PlayerHooks.Player_isSlugpup` hook uses, found by re-reading its decompiled
  source (`reference/ExampleMods/Pupify/Pupify.decompiled.cs`) after the user reported the old
  flip-based version looked "like a slugpup" instead of "a fatter version" — i.e. it lost the real
  character's own `SlugCatClass`-gated body shape and abilities while pup-ified, which is exactly
  backwards from the gimmick's whole point. `SlugpupStatsGimmick` (Postfix on `Player.NPCStats`'s
  constructor) and `SlugpupMalnourishmentStatsGimmick` (Postfix on `Player.SetMalnourished` — a
  second, previously-undiscovered bug found in the same pass: that method unconditionally rebuilds
  `npcCharacterStats` from scratch any time malnourishment status changes, silently undoing the
  rescale) both call into the shared `SlugpupStatsScaling.Rescale` helper to re-derive the pup's
  physical stats from the real character's own stats. `SlugpupColorGimmick` (Postfix on
  `Player.ShortCutColor()`) shrank in scope now that the main body color is automatically correct
  once `SlugCatClass` isn't flipped, but still fixes the narrower shortcut/pipe-transition-tint
  case, which reads `isSlugpup` directly. **Added the same day, a fourth feature, not a bugfix,
  revised twice after direct user corrections:** `SlugpupEndingHook` (Postfix on
  `RainWorldGame.BeatGameMode`, checking `!standardVoidSea` -- i.e. any "standard" ending, not the
  separate void-sea/ascension outcome; see "Slugpup gimmick: ends at Outer Expanse, in detail"
  below for both corrections that got it here, including an intermediate version that wrongly
  hooked `MoreSlugcats.MSCRoomSpecificScript.OE_GourmandEnding.Update` and only caught the Outer
  Expanse walk-ending, not Rivulet/Artificer/Saint/Spearmaster's own alt endings) plus
  `SlugpupEndingMarker` (a permanent per-character save marker, same `unrecognizedSaveStrings`
  mechanism as `OneSpearGimmick`) and `SlugpupEndingSettings` (a new `Configurable<bool>
  EndsOnBeatingGame`, default **on**) — once a character beats the game via any standard ending
  (Outer Expanse's walk-ending, or any character's own alt ending), the gimmick permanently stops
  applying to them (still shown assigned in Remix, just inert) for the rest of that save file. This
  needed no changes to `SlugpupStatsGimmick`,
  `SlugpupMalnourishmentStatsGimmick`, or `SlugpupColorGimmick` at all — they (and vanilla's own
  `NPCStats`-construction gate) all key off `Player.isSlugpup`, so making `SlugpupGimmick`'s own
  Postfix the single place that checks the ending marker is enough to turn the whole gimmick's
  effects off from one choke point. See **Slugpup gimmick stats rescaling, in detail**, **Slugpup
  gimmick: SlugCatClass-flip rewrite, in detail**, and **Slugpup gimmick: ends at Outer Expanse, in
  detail** below before touching any of this — genuinely one of the trickier gimmicks in this
  codebase, and **not yet confirmed working as of this doc** (no `.NET` toolchain in this
  container to build/test against).
- `Gimmicks/OneSpearGimmick.cs` — grants (or, on real Spearmaster, restricts) the tail spear-pull.
  Two parts: `SpearIdentity` flips `SlugCatClass` to `Spear` (same trick as Slugpup) via two hooks
  (`GetInitialSlugcatClass` Postfix and `InitiateGraphicsModule` Prefix — **both are required**,
  see the file's own doc comment for the timing race between them); `OneSpearGimmick` itself
  Prefixes `PlayerGraphics.TailSpeckles.setSpearProgress` to allow the charge to complete up to a
  **configurable limit** of times per save file (default 1, was hardcoded to exactly 1 before this
  UI-overhaul phase), using indexed unrecognized-string markers in `DeathPersistentSaveData` to
  persist the count permanently (`OneSpearSettings`, same file — see below for the settings
  pattern; the marker format changed to be index-per-use, `ALTCHAR_ONESPEAR<dpB><charID><dpB><N>`,
  so a save file that had already spent its one pull under the *old* un-indexed marker will read as
  0 uses under the new scheme — judged not worth migrating, mod still in active dev, no real saves
  at stake). Shown in the UI as **"Master Spear"** — the internal ID/class name is unchanged
  (`"OneSpear"`) to avoid disturbing existing default assignments and save markers. **This file was
  the site of the long-running "grab freezes the game" bug** — see Gotcha #1, now fixed.
- `Gimmicks/TimeLimitGimmick.cs` — kills the character outright once the configured time limit
  (`TimeLimitSettings`, same file: minutes + a per-cycle/per-run toggle, default 60 min / per-run)
  has elapsed, measured against either `RainWorldGame.clock` (per-run) or `World.rainCycle.timer`
  (per-cycle) at the fixed 40 ticks/second simulation rate. Postfixes `Player.Update(bool)`. No
  warning/game-over screen yet — just an immediate `self.Die()`, guarded by a `ConditionalWeakTable`
  so it only fires once per player. See **Gotcha #3** for how direct access to `.clock`/`.timer`
  and `.Die()` was verified safe. **Untested by the user as of this doc.**
- `Gimmicks/NoHoldGimmick.cs` (new, 2026-09-18, replacing the deleted `NoSpearsGimmick.cs` --
  generalization requested directly: "No Hold - generalization of the No Spear gimmick... The
  config inside should allow to toggle Spears, Rocks, Bombs, Lanterns, Fruits, Batflies,
  Squidcadas, Yeeks, Centipedes, Other") — makes any of ten categories of object ungrabbable for
  the assigned character, by Postfixing `Player.Grabability(PhysicalObject)` (same mechanism
  `NoSpearsGimmick`/`NoYeeksGimmick`/`NoCicadasGimmick` already used) and forcing the result to
  `Player.ObjectGrabability.CantGrab` for any object matching a category this character currently
  has checked in `NoHoldSettings` (same file: one `Configurable<bool>` per category, plus a tenth
  "Other" toggle for everything not covered by the first nine). Category membership is a plain C#
  `is` check against the real vanilla class, confirmed class-by-class against `reference/GameSource`
  rather than guessed — see the file's own header comment for the full category-to-class mapping
  and the reasoning behind each judgment call (e.g. why `WaterNut` falls under Rocks not Fruits,
  why `PuffBall`/`FirecrackerPlant` fall under Other not Bombs). Unlike the old `NoSpears`, the
  three individual spear sub-types (plain/explosive/electric) are no longer separately
  configurable — Spears is now one category, per the user's own explicit "we wont be needing this
  anymore." Spears defaults to blocked (matching `NoSpears`' own old default); the other nine
  default unblocked. `GimmickDefaults.cs`'s two old `NoSpears` default assignments (Spearmaster,
  Inv) were migrated to `NoHold` in place, preserving their existing behavior exactly (both
  characters still can't pick up spears by default; nothing else changed for them). Does not
  interfere with `OneSpearGimmick`'s tail-pull, which attaches its spear via
  `Player.SlugcatGrab(...)` — a different, lower-level method that never calls `Grabability`.
  **Not yet built or tested as of this doc** (no `.NET` toolchain in this container — see
  "Container environment notes"; written by close analogy to `ChainsSettings`' own already-shipped
  checkbox-grid UI pattern, but genuinely unverified).
- `Gimmicks/ChainsGimmick.cs` (new, 2026-09-12) — the Chains gimmick: entering a room with any
  living, non-banned creature in it seals every one of that room's exits shut (flips every
  `ShortcutData.Type.RoomExit` in `Room.shortcuts` to `DeadEnd`) and traps the player there,
  visualized with `MoreSlugcats.VoidChain`s running from every sealed doorway to up to 3 of the
  room's creatures, until everything in the tracked list is dead (or a configurable per-character
  escape timer runs out — this port's own addition, not in the source mod, default 90s; see below).
  Ported from a **local, not-on-the-Workshop** BepInEx mod already installed on the user's own
  machine — see **Chains gimmick, in detail** below for the full reverse-engineering trace and why
  the initial Workshop search for "Artificer Chains" found nothing. Postfixes `Player.Update(bool)`
  (same hook shape as TimeLimitGimmick), keeping one `ConditionalWeakTable<Player, ChainState>` of
  per-player lock state. **Brand new, never built or tested as of this doc** — genuinely riskier
  than this codebase's other gimmicks if something's wrong, since a bug here could mean sealed exits
  that never reopen; the escape timer exists specifically to bound that risk until it's confirmed.
- `Gimmicks/ThreatSlowdownGimmick.cs` — the Threat Slowdown gimmick: as vanilla's own threat number
  (the same one the "Threat Music Pulse" Remix option's HUD circles read) climbs from 0 toward the
  point where one full pulse-circle is lit ("threat level 1", derived as `1.1f / 5f == 0.22` from
  `ThreatPulser`'s own rendering math), there's a rising chance of that tick's real input being
  dropped entirely, reaching guaranteed lockout at threat level 1. Postfixes `Player.checkInput()`.
  **Renamed/re-implemented 2026-09-13** — this file originally implemented a percentage-based
  movement/climbing/swimming/throwing-accuracy debuff instead (Prefixing `Player.Update(bool)`,
  with a `ConditionalWeakTable<SlugcatStats, Baseline>` cache); that mechanic is retired, and the
  input-drop mechanic now here used to live in `ThreatPanicGimmick.cs`. See **Threat Slowdown and
  Threat Panic gimmicks, in detail** below for the full account, including why the two files' code
  was swapped rather than their `GimmickDef` IDs renamed. **Not yet built or tested since the
  swap** (the pre-swap input-drop mechanic itself was confirmed working in-game under its old ID).
- `Gimmicks/ThreatPanicGimmick.cs` — the Threat Panic gimmick: as the same threat number climbs
  toward threat level 1, the character starts involuntarily firing off random inputs (one of 4
  directions, grab, throw, or jump, held 1-10 ticks) on top of the player's own real input, which is
  never blocked — see the "Frequency" per-character setting (`PanicSettings`, new this round) for
  how often. Postfixes `Player.checkInput()`; tracks active forced inputs per-player via
  `ConditionalWeakTable<Player, List<PanicInstance>>`. **Renamed/re-implemented 2026-09-13** — this
  file originally implemented the input-drop mechanic now in `ThreatSlowdownGimmick.cs`; see that
  bullet and the detail section below for the full account. **Not yet built or tested since the
  swap.**
- `Gimmicks/CreatureKillListGimmick.cs` (new, 2026-09-15; revised same day) — the Kill List gimmick:
  tracks every one of a fixed, curated 55-species roster (`KillListRegistry.All`) the assigned
  character personally kills — tracking only, no goal/reward logic yet. `CreatureKillListGimmick`
  itself (a Postfix on `Creature.Die()`) reads vanilla's own `Creature.killTag` for attribution
  rather than reimplementing "who killed this", and records into `KillListTracker`'s in-memory
  "pending" set (not the save file directly). `CreatureKillListCycleWinHook`/
  `CreatureKillListCycleGameOverHook` (Postfixes on `RainWorldGame.Win`/`.GameOver`) decide each
  cycle's fate: `Win` (confirmed via decompilation to fire only from `ShelterDoor.cs`, i.e. only on
  a genuine successful shelter-reach) commits a character's pending kills to
  `DeathPersistentSaveData.unrecognizedSaveStrings` (same marker mechanism `OneSpearGimmick.cs`
  already established); `GameOver` (confirmed to fire only from real death paths in `Player.cs`)
  discards them instead — "if character dies, then it should not get counted," per explicit
  request. `KillListButtonHook`/`KillListSignalHook` (Postfixes on `Menu.SleepAndDeathScreen`'s
  `AddSubObjects`/`Singal`) add a "KILL LIST" button to the cycle-end screen that opens a
  `Menu.DialogNotify` listing not-yet-killed species, then killed — replacing an earlier
  Remix-settings-based progress view entirely, per explicit request. `CreatureKillListHandlerHook`
  (Postfix on `PhysicalObject.Grabbed`) records "who last grabbed this creature" as a fallback
  attribution source for death causes with no `killTag` at all — added after double-checking the
  leech kill path specifically found that Leech isn't `IPlayerEdible` and dies via its own
  unattributed `airDrown` (out-of-water) mechanism, the single most common way a leech actually
  dies in normal play. See **Kill List gimmick, in detail** below for the full account, including
  two more attribution gaps found but deliberately not yet fixed (throw-impact kills for species
  whose own `TerrainImpact` override applies lethal damage, and slow spear-stab kills outlasting
  `killTag`'s own decay window). **Brand new, never built or
  tested as of this doc.**
- `Gimmicks/RainTimerGimmick.cs` (new, 2026-09-18; render-bug fix and Saint defaults added later
  same day) — the Rain Timer gimmick: rescales how long rain (or, on Saint, the region-wide
  blizzard) takes to arrive each cycle, as a configurable percentage of vanilla's own normal timing
  (`RainTimerSettings`, same file: one `Configurable<int>` percent, 0-400%, default 100 =
  unmodified, 0 = essentially immediate). Postfixes `RainCycle`'s constructor (the *only* place vanilla
  itself ever computes `cycleLength`, confirmed via `GetDesiredCycleLength` having exactly one
  caller) and rescales the `cycleLength` it already computed, plus `sunDownStartTime` by the same
  ratio so day/night lighting doesn't drift out of sync with how fast the actual rain/blizzard
  danger is arriving. This is the same pair of fields a real, already-installed third-party mod
  (MMF) scales for its own "rain time multiplier" config — confirmed by reading `RainCycle.cs`'s
  own `ModManager.MMF`-gated lines rather than guessing, so this gimmick mirrors an
  already-working approach instead of inventing a new one. **Works identically for Saint's blizzard
  with no extra plumbing**, because `BlizzardWorldActive` (true only on Saint) is read *inside*
  `GetDesiredCycleLength` itself, before this gimmick's own Postfix ever runs — Saint's blizzard
  isn't a separate timer, it's the same `RainCycle`, just pre-shortened to 70% of normal by
  vanilla's own code before this gimmick's percentage multiplies on top of that.
  **The known "blizzard doesn't render" bug — root-caused AND FIXED (2026-09-18, later same day),
  not just floored around:** the user reported that an earlier, non-mod attempt at this same
  feature found the blizzard stopped rendering once the timer dropped below roughly 13% of its
  normal length, and asked this to actually be resolved rather than just avoided. Root-caused by
  reading `MoreSlugcats.BlizzardGraphics.CycleUpdate` directly: for Saint's own home-region ambient
  blizzard (the "glacial wasteland" branch — `AerieBlizzard`-flagged rooms use fixed absolute-tick
  thresholds instead and were never at risk), blizzard intensity is
  `Mathf.InverseLerp(cycleLength * 0.932f, 3000f, TimeUntilRain)`; once `cycleLength` drops below
  `3000 / 0.932 ≈ 3220` ticks (~80.5 seconds), `Mathf.InverseLerp`'s two bounds invert and the ramp
  runs BACKWARDS instead of clamping flat — a small nonzero intensity right at cycle start,
  collapsing to a clamped 0 well BEFORE rain/blizzard actually hits (worked out by hand, not
  guessed — see the file's own header comment, "The known 'blizzard doesn't render' bug," for the
  full arithmetic). **The actual fix is `BlizzardVisualCycleLengthHook`**, a separate, unconditional
  Prefix+Postfix pair directly on `BlizzardGraphics.CycleUpdate` (not gated on any gimmick) that
  temporarily substitutes a safe floor (4200 ticks) for `cycleLength` for the span of that one call
  — confirmed to be the *only* method anywhere that reads `cycleLength` for rendering — then
  restores the real value immediately after. This has zero effect on unmodified vanilla and only
  ever engages once something has pushed the real gameplay `cycleLength` below the safe rendering
  threshold. With the real bug fixed, `RainTimerGimmick.MinCycleLengthTicks` (the gameplay-side
  floor) dropped from 4200 down to 40 ticks (1 second) — now purely an anti-division-by-zero guard,
  not a render-bug workaround, so `RainTimerSettings.MinPercent` could drop from 10 to 0 too. **Now
  defaulted to Saint at 0%** (`RainTimerSettings.SaintDefaultPercent`, special-cased inside `Bind()`)
  and assigned to Saint by default in `GimmickDefaults.cs`, per explicit request ("it needs to
  start immediately"). **Not yet built or tested as of this doc** (no `.NET` toolchain in this
  container) — the render-bug fix in particular has never been seen in-game.
- `Gimmicks/BlizzardTimerGimmick.cs` (new, 2026-09-18) — the Blizzard Timer gimmick: controls how
  many minutes into the cycle (from the cycle's own start, not from whenever rain/blizzard actually
  hits) the blizzard's heat-drain mechanic suddenly jumps from a gentle background rate to its
  maximum severity. Traced to `Creature.HypothermiaUpdate()`: vanilla computes a threshold tick
  `num3 = cycleLength + RainWorldGame.BlizzardHardEndTimer(storyMode)` (the latter a hardcoded
  32000-tick/13m20s constant in story mode), and the instant `rainCycle.timer` (which keeps counting
  up even after rain/blizzard has already hit) crosses `num3`, per-tick cold gain jumps from a
  gentle ramp straight to the method's own hard ceiling (`Mathf.Clamp(..., 0.0055f)`) -- effectively
  instant, matching the user's own "suddenly gets significantly harsher" framing exactly. **Unit,
  per the user's own explicit choice after being shown this mechanism: "Total minutes into the
  cycle"** (`BlizzardTimerSettings`, same file: one `Configurable<int>` minutes, default 25,
  approximating -- not reproducing exactly, vanilla has no single constant for this -- the vanilla
  feel the user described). Implementation is a Postfix on `RainWorldGame.BlizzardHardEndTimer`
  (confirmed via full-codebase grep to have exactly one call site anywhere in the game, so patching
  it can't affect anything else) that overrides its return value to `(configured total ticks) -
  cycleLength`, clamped to never go negative. Since that static method takes no Player/Creature
  parameter at all, a second Prefix+Postfix pair on `Creature.HypothermiaUpdate()` itself
  (`BlizzardTimerHypothermiaGateHook`) stashes/clears which Creature is currently mid-call in a
  shared static field (`BlizzardTimerGimmick.CurrentCreature`) so the `BlizzardHardEndTimer` Postfix
  can resolve "which player, if any, is asking" despite the method it's patching having no such
  context -- safe because Rain World's simulation is single-threaded and this method never
  recurses. Deliberately does NOT touch `cycleLength` directly (the more obvious-looking approach):
  that field is read a second time a few lines later in the same vanilla method for an unrelated
  wind-chill sub-term, so temporarily substituting it would have silently thrown that off too.
  Composes correctly with Rain Timer above -- both resolve against the character's actual live
  `cycleLength` at the moment they're asked, so assigning both to the same character means exactly
  what each one's own setting independently says, no double-counting. **Now defaulted to Saint at 60
  minutes** (`BlizzardTimerSettings.SaintDefaultMinutes`, special-cased inside `Bind()`, 2026-09-18
  later same day) and assigned to Saint by default in `GimmickDefaults.cs`, per explicit request.
  **Not yet built or tested as of this doc** (no `.NET` toolchain in this container) -- also genuinely the
  riskiest of today's three gimmicks to verify by inspection alone, since it hooks a Postfix on a
  method with no natural per-call context and relies on a stashed-static-field handoff between two
  separate patches; test this one particularly carefully once a build is available (assign it to a
  character, confirm the cold-gain spike now happens at the configured total-cycle-minute mark
  instead of vanilla's own regionally-varying default, and confirm nothing else that reads
  `RainWorldGame.BlizzardHardEndTimer` or `Creature.HypothermiaUpdate` regressed for a character
  that does NOT have this gimmick assigned). **Later same day:** both this gimmick and Rain Timer
  gained a "Disable timer" checkbox next to their numeric field (greys out the field+label when
  checked, substitutes a large-but-bounded sentinel instead of the normal math) -- see the "Disable
  timer checkboxes... Precycle Timer" dated section further down for the full account.
- `Gimmicks/PrecycleTimerGimmick.cs` (new, 2026-09-18, later still; flood chance and "Force max
  precycle timer" added in a further follow-up the same day) — the Precycle Timer gimmick: replaces
  vanilla's own precycle ("shelter failure") decision outright -- frequency (a flat 0-100% for the
  assigned character, replacing vanilla's own per-region `earlyCycleChance`), head-start duration
  (a fixed configured number of minutes, replacing vanilla's own random 2-5 minute roll -- or,
  "Force max precycle timer" checked, always the longest configurable duration instead, modeled on
  Remix's own `MMF.cfgNoRandomCycles`), and flood chance (a flat 0-100%, replacing vanilla's own
  per-region `earlyCycleFloodChance`, faithfully preserving vanilla's one MS-region/Rivulet
  exception -- see the file's own header comment for why that one exception was kept rather than
  folded into the "full override" treatment). A Postfix on the same `RainCycle` constructor
  `RainTimerGimmick` already Postfixes, touching entirely disjoint fields
  (`maxPreTimer`/`preTimer`/`preCycleRainPulse_Wave*`/`globalRain.drainWorldFlood` vs
  `cycleLength`/`sunDownStartTime`), so the two compose with no ordering dependency. See the file's
  own header comment for the full trace, including region-coverage findings checked directly rather
  than assumed (the timer mechanism itself is confirmed region-agnostic; the flood's VISUAL effect
  is not -- it's additionally gated on each room's own level-design `RoomRain.DangerType`, a genuine
  vanilla limitation this gimmick can't route around), and the "Precycle Timer gains flood chance...
  region coverage confirmed" dated section further down for the full account, including the vanilla
  variance numbers (400-800 second base cycle length, 4800-12000 tick precycle duration) this
  gimmick's own design was checked against. No default assignment to any character. **Not yet built
  or tested as of this doc** (no `.NET` toolchain in this container) — genuinely novel among this
  mod's gimmicks in that it re-derives and overrides vanilla RNG rolls rather than just reading or
  gating on existing state, so treat it with extra scrutiny once a build is available: confirm
  precycles actually stop happening at 0% even in a normally precycle-prone region, confirm they
  happen (eventually) at 100% even in a region that normally never has them, confirm the configured
  duration (or, with force-max checked, always the longest one) is what actually shows up as the
  pre-rain head start in-game, and confirm flood chance actually raises water in a room known to
  have a flood-capable `DangerType` while having no visible effect in one that doesn't.
  **Later still the same day:** two more settings -- "Force Submerged Superstructure Precycles"
  (skips the MS/Rivulet flood exception for everyone) and "Force shelter unavailable" (forces
  `World.brokenShelters`/`brokenShelterIndexDueToPrecycle` directly via a brand new second Harmony
  patch, `PrecycleForceShelterUnavailableHook`, a Prefix on `ShelterDoor`'s own constructor --
  completely separate from this gimmick's `RainCycle`-constructor Postfix, since it needed a
  different hook point entirely). See the "Precycle Timer gains Force Submerged Superstructure...
  shelter unavailable" dated section further down for the full account, including why a real vanilla
  mechanic already existed for this (just scoped to only the shelters a region's own level design
  specifically marked) and how this hook forces it everywhere instead.

### Deployment layout (all on the user's Windows machine, not this container)

- Source: `C:\Users\rokas\Documents\RWMods\src\Primal\` (this file's sibling location).
- Reference DLLs used to compile against: `C:\Users\rokas\Documents\RWMods\lib\`.
- Installed mod: `C:\Program Files (x86)\Steam\steamapps\common\Rain World\RainWorld_Data\
  StreamingAssets\mods\Primal\` — `modinfo.json` + `plugins\Primal.dll`. This is
  what BepInEx actually loads; after every rebuild, the new DLL has to be copied here (see
  **Build & deploy workflow**).
- Game root: `C:\Program Files (x86)\Steam\steamapps\common\Rain World\` — also where
  `exceptionLog.txt` and `consoleLog.txt` live (Unity's own crash/exception logs — **check these
  before theorizing** when a bug is "silent", see Gotcha #1), and `BepInEx\LogOutput.log` (BepInEx's
  own log, shows plugin load order and any `Logger.LogError` calls this mod makes).
- No longer requires SlugBase. The hard `[BepInDependency]`/`modinfo.json` `requirements` on
  `slime-cubed.slugbase` was removed 2026-09-15 (see the refactor entry near the bottom of this
  doc) — it was a leftover from the two discarded SlugBase-character designs, controlling nothing
  but BepInEx's plugin load order for a dependency this mod stopped using. SlugBase can still be
  installed alongside Primal with no conflict; it's just no longer required.
- `scripts/build-deploy-run.sh` (Git Bash, run on the Windows machine above — not in this
  container) does the build + copy-to-`MOD_DEST` + launch cycle described above in one step, with
  `--no-build`/`--no-launch` flags for a partial run. Its default paths match the ones just above;
  override via `PRIMAL_SRC_DIR`/`RAINWORLD_GAME_DIR`/`RAINWORLD_MOD_DEST` env vars if they ever
  drift from what's documented here.

## Gotcha #1 — direct field/property access can compile fine and still crash at runtime

The compile-time reference assembly (`lib/PUBLIC-Assembly-CSharp.dll`) is a "publicized" copy of
the game's real assembly, with every field/method's accessibility forced to `public` for
convenience. **The actual assembly running on the user's machine still enforces the ORIGINAL
(non-public) accessibility.** Code that directly reads/writes a field that's non-public in the real
game (e.g. `PlayerGraphics.player`) compiles cleanly against the publicized reference, but throws
`FieldAccessException` at actual runtime once Harmony/MonoMod JITs the patch against the real
assembly.

This is exactly what caused the long-running "Spearmaster's grab freezes the game" bug, misdiagnosed
several times earlier (as session-identity timing, SlugCatClass-flip races, etc. — all wrong).
`OneSpearGimmick.cs`'s `Prefix` on `setSpearProgress` did `__instance.pGraphics.player`, and
`PlayerGraphics.player` is non-public in the real assembly. The exception threw silently, every
tick, once spear-charge neared completion, and the game's per-creature update loop swallows
per-tick exceptions without crashing — so the character just stopped responding to input, with
nothing informative in BepInEx's own log. **Found via Unity's `exceptionLog.txt`** (at the game
root), not by reading code. Fixed by reading the field through reflection instead
(`HarmonyLib.AccessTools.Field(typeof(PlayerGraphics), "player")`, cached, `.GetValue(...)`),
which bypasses the JIT-level accessibility check.

**Lesson, stated plainly for next time:** if a character "just stops responding to input" or
something "silently doesn't work" with nothing in BepInEx's log, stage and read
`exceptionLog.txt`/`consoleLog.txt` from the game root FIRST, before theorizing from code review.
Also: **audit for this same class of bug** wherever this codebase directly accesses a field or
property on a vanilla type via `.` — if it isn't something this mod's own Harmony attribute already
targets by name (which goes through reflection and is safe), check whether the member is actually
public in the real game before assuming the publicized reference DLL's accessibility is real.
Method calls Harmony patches by name (`nameof(...)` in a `[HarmonyPatch]` attribute) are **not**
subject to this — Harmony resolves those via reflection, not a compiled call/field instruction, so
existing patches on private methods like `GetInitialSlugcatClass` are fine. Likewise, naming a type
in a `[HarmonyPatch(typeof(X), ...)]` attribute (including a type most or all of this mod's other
code has never referenced before) is fine regardless of that type's real accessibility — it's a
type-token lookup, not a member access, and Harmony's whole job is patching non-public game types.

**2026-09-11 addendum — `reference/GameSource/` has the exact same false-"public" problem as
`lib/PUBLIC-Assembly-CSharp.dll`, because it was decompiled from that same publicized copy, not
from the real assembly.** Confirmed directly: `reference/GameSource/PlayerGraphics.cs` shows
`public Player player;` — the very field this Gotcha proved is non-public in the real game. So
when reading `reference/GameSource/` to find a new field/property to access directly (as opposed to
finding a *method* to Harmony-patch by name, which is unaffected), don't trust an access modifier
shown there any more than one shown in the publicized DLL itself — the same "assume it's private
until cross-checked (Gotcha #3) or accessed via reflection" discipline applies. This is exactly why
`PrimalCharacterNames.cs` (see below) reads `SlugcatPageNewGame.difficultyLabel`/`.infoLabel` via
`AccessTools.Field(...).GetValue(...)` instead of a plain `__instance.difficultyLabel` — both
fields show as `public` in `reference/GameSource/Menu/SlugcatSelectMenu.cs`, which this addendum
means proves nothing either way.

## Gotcha #2 — game-registered content isn't safely readable from `Plugin.OnEnable`

`MoreSlugcatsEnums.SlugcatStatsName.Gourmand`/`.Spear` and `WatcherEnums.SlugcatStatsName.Watcher`
are `ExtEnum` values the game itself registers (`RegisterValues()`) during its own startup — not
compile-time constants. Reading `.value` on them from `Plugin.OnEnable()` (BepInEx's Chainloader
stage, before any `RainWorld` instance exists) throws `NullReferenceException`. This previously
happened inside `GimmickDefaults.Apply()`, which used to be called first thing in `OnEnable()`,
and because nothing caught it, it aborted `OnEnable()` before `_harmony.PatchAll()` ever ran —
**silently disabling every single Harmony patch in the mod**, including the PRIMAL SLOT button
and all gimmicks, with no error in BepInEx's log (just a missing "loaded" line, easy to miss).

Fix, now in place: `GimmickDefaults.Apply()` and the Remix-screen registration both moved to a
Postfix on `RainWorld.OnModsInit` (`RainWorldHooks.cs`) — the standard, documented point by which
all game-registered enums and `MachineConnector`'s per-mod OI slots already exist. `Plugin.OnEnable`
now does nothing but `_harmony.PatchAll()`, wrapped in try/catch with `Logger.LogError` so a future
failure here is at least visible in the log instead of vanishing.

**Lesson:** anything that touches `MoreSlugcatsEnums`, `WatcherEnums`, or similar game-registered
content must not run from `Plugin.OnEnable`. Use `RainWorld.OnModsInit` (or later).

## Gotcha #3 — verifying real accessibility without a live test: cross-check other installed mods

Gotcha #1 means every new direct field/property/method access against a vanilla type is a small
question mark until proven otherwise, but this container has no way to actually run the game to
check. **A cheap, reliable substitute: disassemble another mod that is already installed on the
user's machine and already known to work, with `ikdasm`, and grep it for the same member access.**
If a real, currently-running mod already does the exact same direct access (not through
reflection), that member is genuinely accessible on this exact game version — no guessing needed.

This was used to greenlight `TimeLimitGimmick.cs` and `NoSpearsGimmick.cs`'s direct (non-reflection)
use of `RainWorldGame.clock`, `Creature.Die()`, and `Player.Grabability(PhysicalObject)`:
- `RainWorldGame.clock` (plain field read) and `Creature.Die()` (plain virtual call) both appear
  directly in **Rain Meadow** (workshop item `3388224007`, `plugins\Rain Meadow.dll` — a large,
  actively maintained multiplayer mod, a good source of "this API is definitely fine" evidence
  precisely because it's large and mature).
- `Creature.get_dead()` (the `.dead` property) appears directly in **Pupify** (workshop item
  `3005429221`, `plugins\Pupify.dll` — confirmed independently as actually running on this
  machine, since its `PlayerHooks.Player_GrabUpdate` showed up in the exceptionLog.txt stack trace
  during the Gotcha #1 investigation).
- `Player.Grabability(PhysicalObject)` itself wasn't cross-checked this way (no other installed mod
  happened to touch it) — this one is a judgment call based on it being a `public`, non-`virtual`,
  ordinary instance method with a normal descriptive name, extensively called from many places
  within `Player`'s own class. Flag this as the one part of these two new gimmicks that's least
  independently verified, if `NoSpearsGimmick` is reported as silently not working.

**How to repeat this technique for a new member access:** get the other mod's workshop item ID
(cross-reference `modinfo.json`'s `"name"` field across
`C:\Program Files (x86)\Steam\steamapps\workshop\content\312520\<id>\modinfo.json` — request
folder access to the whole `...\312520` directory once, then `device_list_dir` a candidate ID's
`plugins\` folder, `device_stage_files` the `.dll`, then `ikdasm <path> > out.il` and `grep` for
the member). `ikdasm` is already present on this container. Do not bother trying
`dotnet tool install ilspycmd` — this container's proxy blocks `api.nuget.org` (confirmed via
`curl "$HTTPS_PROXY/__agentproxy/status"` showing repeated `connect_rejected` to it), so no NuGet
package installs work here at all.

## Gotcha #4 — a Configurable&lt;T&gt; can only ever be bound to one UIconfig element at a time

Discovered the first time the user actually opened the new drill-down config menu in Remix (see
"Config menu drill-down UI, in detail" below): every view worked the first time through, but
opening the **same** gimmick's settings sub-view a **second** time (e.g. back out to the character
table, then back into "No Spears" settings for Spearmaster again) crashed the whole
`OptionInterface` with `Menu.Remix.MultiuseConfigurableException: Configurable
Spear_NoSpears_allowNormal is bound to multiple UIconfigs`.

Root cause: `GimmickDef.BuildSettingsUI`'s original contract said to build a **fresh**
`List<UIelement>` (new `OpCheckBox`/`OpUpdown` instances included) every single time the settings
sub-view opened, on the assumption that removing the old elements from the `OpTab` via
`RemoveItems()` was enough cleanup. It isn't: a `Configurable<T>` keeps an internal reference to
whichever UIconfig-derived control (`OpCheckBox`, `OpUpdown`, etc.) is currently bound to it, and
`OpTab.RemoveItems` only detaches an element from the tab's own display list — it does **not**
unbind the element from its `Configurable`. So the second time `BuildSettingsUI` ran for the same
(character, gimmick) pair, it constructed a brand-new `OpCheckBox` against a `Configurable<bool>`
that was still bound to the *first* `OpCheckBox` instance from the earlier visit — and the
`UIconfig` base constructor throws `MultiuseConfigurableException` the instant that happens.

**Fix, now in place:** every `*Settings` class (`TimeLimitSettings`, `NoSpearsSettings`,
`OneSpearSettings`) lazily builds its `List<UIelement>` once per character and caches it in a
`Dictionary<string, List<UIelement>>`, returning the exact same instances on every later call
instead of constructing new ones. `GimmickDef.cs`'s own doc comment on `BuildSettingsUI` was
corrected to state this as a hard requirement (it previously said the opposite — "do not cache" —
which was the bug). `OpLabel` instances (not `Configurable`-backed) would have been safe to
rebuild, but for simplicity the whole returned list is cached together per class.

**Lesson:** any `UIelement` that wraps a `Configurable<T>` (`OpCheckBox`, `OpUpdown`, `OpSlider`,
etc.) must be constructed **at most once** per `Configurable` for the life of the `OptionInterface`
instance. A drill-down/paged Remix UI that needs to show the same setting more than once (including
"the same view, revisited") must build it once and cache+reuse the element — never rebuild it fresh
from the `Configurable`. **This fix itself is untested as of this doc** — deployed, not yet
re-opened in Remix by the user.

## Gotcha #5 — a stale, un-rebuilt DLL silently masks every source change, and looks exactly like a broken feature

**2026-09-11, later session:** the user reported the Primal-names feature "did not work" at all —
"Spearmaster on alt slot is still called Spearmaster. Nothing has changed." First checked whether
Gourmand/Spearmaster already had save progress on the alt slot (which would mean
`SlugcatPageNewGame` — the only page with a name/description to overwrite — never runs for them
any more; see "Primal character names, in detail") — **user confirmed no, both are fresh/no save
yet**, ruling that out. So the page this patch targets should be showing.

Checked build artifact timestamps in the connected `RWMods` folder instead of theorizing further
from code (same "check the evidence before re-reading your own code" discipline as Gotcha #1):

| File | Last modified (UTC) |
|---|---|
| `src/Primal/bin/Debug/Primal.dll` (local build output) | **2026-09-10 18:18** |
| `src/Primal/mod/plugins/Primal.dll` (csproj's own post-build copy) | **2026-09-11 04:47** |
| `PrimalSlot.cs` through `OneSpearGimmick.cs` (that day's earlier gimmick/UI fixes) | 2026-09-11 10:03 – 12:30 |
| `Gimmicks/PrimalOptions.cs` (the character-grid/detail-panel UI merge) | 2026-09-11 12:44 |
| `Gimmicks/PrimalCharacterNames.cs` (this new file) | 2026-09-11 12:56 |

**Both locally-built DLL artifacts predate every single source change made that day** — including
the two drill-down bugfixes, the UI merge, and the new file entirely. There is no way a DLL built
at 04:47 (or earlier) contains code that didn't exist until 10:03–12:56. This is a far more likely
explanation for **both** "failed" reports than a logic bug in the new source:
- The UI merge "still behaved like the old drill-down" — because the DLL actually running **was**
  the old drill-down; the merge's source change was never compiled into what got tested.
- Primal names did literally nothing — same reason, one level further: that source file didn't
  exist yet when whatever DLL is currently installed was built.

**This does not by itself prove the new code is correct** — it only means the new code has
plausibly never actually been tested at all yet. Don't skip real verification once a fresh build is
confirmed; this just means the most likely next step is "rebuild and redeploy, then retest," not
"re-read PrimalCharacterNames.cs looking for a bug."

**How to check this quickly in a future session, before assuming a "doesn't work" report is a code
bug:** compare `src/Primal/mod/plugins/Primal.dll`'s `mtimeMs` (via
`device_list_dir`) against the newest `.cs` file's `mtimeMs` in the same tree. If the DLL is older
than source files relevant to the reported bug, the fix is "rebuild and redeploy" before any code
changes, not a debugging session. This only checks the **local** copy inside the source tree — the
actually-loaded copy is a separate file at `C:\Program Files (x86)\Steam\steamapps\common\Rain
World\RainWorld_Data\StreamingAssets\mods\Primal\plugins\Primal.dll`, outside the
`RWMods` connected folder, that can be even further out of date than the local one if the user
forgot the last manual copy step too. **Update (2026-09-11, Gotcha #6's retest):** this path is
reachable this session via `device_request_folder_access` on the Steam game root (see Gotcha #6) —
once granted, this session compared this file's size/mtime directly against the local
`mod/plugins/Primal.dll` and found them mismatched (proving the user's manual copy step
hadn't happened), then **wrote the correct DLL there itself** with `device_commit_files`, verified
by reading the size/mtime back afterward. So a future session doesn't have to just *ask* the user to
copy the file and hope — if game-root folder access is already granted (or grantable), copy the
built DLL there directly and confirm the copy landed, which removes an entire class of "rebuilt but
forgot to redeploy" false-negative bug reports. Folder-access grants are per-session, so this may
need re-requesting each session; but it's worth doing before pushing every fix that needs a live
retest, not just once diagnosing this specific bug.

**Lesson:** when a change "does nothing at all" (as opposed to doing the *wrong* thing), stale
build/deploy is at least as likely a cause as a silent logic bug — check build artifact timestamps
before re-deriving the feature's logic from scratch.

**Root cause, confirmed (2026-09-11, same session):** asked the user to run `dotnet build`
themselves and paste the output. It failed — 4x `CS0117: 'PrimalSlot' does not contain a definition
for 'SetCurrentAltCharacterID'`, all inside `Gimmicks/AltCharacterIdentity.cs`. That file is dead
code from the discarded design #2 (9 SlugBase alt characters + forced session identity — see
History) that was never deleted when `PrimalSlot.SetCurrentAltCharacterID` was removed during the
pivot to the current design. **This means the project has not compiled successfully since
whichever earlier session made that removal — every `dotnet build` since then has failed, so the
user has been testing an old DLL for an unknown number of sessions' worth of changes, not just
today's.** Grepping the whole source tree confirmed nothing outside that one file references
`AltCharacterIdentity`/`StoryGameSessionIdentityHook`/`AltCharacterIdentityExitGameHook`, so it's
fully isolated dead code, not something anything else depends on.

**Fix, now in place:** `Primal.csproj` gained `<ItemGroup><Compile Remove=
"Gimmicks\AltCharacterIdentity.cs" /></ItemGroup>` — excludes the file from compilation without
deleting it (this session still has no delete capability on the user's machine; the file itself is
unchanged and the existing Outstanding-items request to delete it by hand still stands). This was
the only compile error the build reported — the full build output showed exactly these 4 errors
and nothing else, meaning **every other current file, including today's UI merge and
`PrimalCharacterNames.cs`, already compiled cleanly** once this one dead file is out of the way.
That's meaningfully de-risking: neither of today's "didn't work" reports needed a source fix at
the code-review level, just this one unrelated build break cleared out of the way first.
**Confirmed fixed (2026-09-11):** the user re-ran the build — `Build succeeded, 0 Warning(s), 0
Error(s)` — and the same command's `Copy-Item` step (silent on success, no output shown, but no
error either) should have put the fresh DLL at
`...\StreamingAssets\mods\Primal\plugins\Primal.dll`. Cross-checked via the source
tree's own post-build copy (`mod\plugins\Primal.dll`, which the csproj also updates on every
build): now 34,304 bytes as of this build, up from 15,360 bytes on the last stale copy — a big
enough jump to be obviously a real rebuild picking up everything added since (UI merge, Primal
names, TimeLimit/NoSpears, etc.), not a no-op. **Not yet confirmed the copy into the actual game
folder landed** (this container doesn't have access to that path to check directly) — next step is
simply: launch the game and test. This is the first time either the UI merge or Primal names has
ever run against a build that actually contains them.

`AccessTools.Field` returning null (see "Primal character names, in detail") was the other leading
hypothesis for the Primal-names symptom specifically — now deprioritized given the build was
broken the whole time, but still worth checking if Primal names still doesn't work once the build
is confirmed green.

**Second, smaller trip-up fixing the above:** the `<Compile Remove>` fix's own explanatory XML
comment in `Primal.csproj` used `--` (this codebase's normal `//`-comment style throughout
the `.cs` files) inside an `<!-- -->` block — which XML forbids (`--` isn't allowed inside a
comment body, and a comment can't end in `-`), so it broke MSBuild's own XML parser entirely
(`MSB4025`, project file fails to *load*, before the compiler even runs — a much more total
failure than a C# compile error, since no `.cs` file's errors get a chance to show). Fixed by
rewording to avoid `--` in that comment. **Lesson:** never reuse this codebase's `.cs`-comment
`--`-as-dash habit inside an XML comment (`.csproj`, `modinfo.json` has no comments so it's N/A) —
use a plain comma, semicolon, or parenthetical instead.

## Gotcha #6 — leftover Harmony patches from a discarded design can be broken by construction, with no external trigger needed

**2026-09-11, same session, right after Gotcha #5's build fix:** asked the user to launch the game
and test both the UI merge and Primal names against the now-genuinely-fresh DLL. Instead of a
direct answer to either check-item, the user reported: *"the character select menu took a while to
load and looks like this"* with two screenshots of an almost entirely blank screen — a dashed
rounded rectangle, a few overlapping rounded shapes, one small black square, and a couple of small
outlined squares/circles near the bottom, no text or character art at all.

Rather than ask which screen this was (Remix options vs. the in-game slugcat-select screen) or wait
for logs (two earlier `device_request_folder_access` attempts at the Steam game root had already
timed out unanswered this session), re-checked what else was still sitting, compiled, in the source
tree from the same discarded design #2 that Gotcha #5's `AltCharacterIdentity.cs` came from —
`Primal.csproj` only excluded that one file; `Characters\*.cs` (9 old SlugBase clone
characters — `AltGourmand`, `AltSpear`, etc. — plus `AllCharacters.cs`) and
`CharacterSelect\*.cs` (`AltRoster.cs`, `SlugcatSelectMenuHooks.cs`) were still being compiled and
loaded like any other file in the mod.

Reading them turned up a bug that's provably broken **by construction, from source alone, with no
need to check what's on disk in the game's `mods\Primal\slugbase\` folder**:

- `SlugcatSelectMenuHooks.cs` is a live `[HarmonyPatch]` on `Menu.SlugcatSelectMenu.Update` — a
  postfix that runs every single frame, stepping `slugcatPageIndex` in a `while` loop until it lands
  on a page whose character counts as "visible" for the currently-mounted save slot
  (`AltRoster.IsVisible`).
- `AltRoster.IsVisible(name, rainWorld)` is defined as `IsAlt(name) == PrimalSlot.IsActive(rainWorld)`.
- `AltRoster.IsAlt(name)` can only ever return true for a character ID some code registered via
  `AltRoster.RegisterAlt(...)` — and the only callers of that method are the 9 `Characters\Alt*.cs`
  static constructors. Grepped `RainWorldHooks.cs` (the mod's actual `OnModsInit` setup) and
  confirmed it only calls `GimmickDefaults.Apply()` and registers the Remix screen — nothing
  anywhere still forces those 9 static constructors to run (no `AllCharacters.All` iteration, no
  `RunClassConstructor`, nothing). **So `AltRoster`'s internal registry is permanently empty, and
  `IsAlt()` always returns false.**
- That makes `IsVisible` reduce to `false == PrimalSlot.IsActive(rainWorld)` — true only while
  `PrimalSlot.IsActive` is itself false. **On this mod's own reserved alt slot** (`IsActive` true,
  exactly the slot needed to test Primal names), **no character in `slugcatColorOrder` ever counts
  as visible**, so `SlugcatSelectMenuHooks`'s `while` loop can never find a page to land on — it
  burns its entire `guard` budget every frame, repeatedly mutating `slugcatPageIndex` and calling
  `UpdateSelectedSlugcatInMiscProg`/`UpdateStartButtonText`, forever, with no valid target to settle
  on.

This was the leading theory from source review alone, before any log was available (two earlier
`device_request_folder_access` attempts at the Steam game root had timed out unanswered this
session). Applied the fix anyway — see below — since it was clearly correct regardless (an empty,
permanently-unpopulated `AltRoster` registry serving a still-active per-frame Harmony patch is a
real bug on its own merits) even before direct confirmation.

**First fix attempt silently did not reach the device.** `Primal.csproj` was edited locally
to add `<Compile Remove="Characters\**\*.cs" />` and `<Compile Remove="CharacterSelect\**\*.cs" />`
and pushed via `device_commit_files`, which reported `"written"` with no errors. The user then ran
`dotnet build` and copied the DLL again and reported **"its the same thing"**.

**Checked real evidence this time instead of re-theorizing (Gotcha #1 discipline) — retried the
folder-access request for the Steam game root, and it was granted** (the user was actively at the
computer this time, unlike the two earlier timeouts). Read the actual `exceptionLog.txt`,
`consoleLog.txt`, and `BepInEx\LogOutput.log` from the game root. `exceptionLog.txt` contained the
real, confirmed root cause:

```
MethodAccessException: Method `Menu.SlugcatSelectMenu.UpdateSelectedSlugcatInMiscProg()' is inaccessible from method `AltCharacters.CharacterSelect.SlugcatSelectMenuHooks.Postfix(Menu.SlugcatSelectMenu)'
```

— a textbook **Gotcha #1** case (direct call to a member that's public in the compile-time
publicized reference assembly but not genuinely public in the real game assembly), except on a
*method* rather than a field, in `CharacterSelect/SlugcatSelectMenuHooks.cs`'s
`self.UpdateSelectedSlugcatInMiscProg();` call. This throws from inside a Harmony postfix on
`Menu.SlugcatSelectMenu.Update`, which runs every single frame the menu is open, and — per the
stack trace — propagates up through the MonoMod trampoline chain of *every other mod's* hook on
that same method (`Pupify`, `Gourmand Stamina Display`, `Artificer Rubicon Hell Bonanza` all also
hook `SlugcatSelectMenu.Update`/`RainWorld.Update`), aborting each of their `Update` calls for that
frame too. Firing every frame, forever, while that menu is open fully explains both symptoms: a
screen that can never finish whatever per-frame setup/animation it's mid-way through (the
near-blank/wireframe look) and "took a while to load" (constant exception unwinding instead of
normal per-frame work). This is independent of whether the leftover per-character SlugBase JSON
under `...\mods\Primal\slugbase\` (see Outstanding items) is still installed or not.

**Why the first fix "did nothing": it never actually landed.** Re-staged
`Primal.csproj` directly from the device after the "same thing" report, before assuming the
fix itself was wrong — and it was still the **old** content (4,089 bytes, only the original
`AltCharacterIdentity.cs` exclusion), not the 6,218-byte version with the two new exclusions that
had been sitting correctly in this session's own workspace the whole time. `device_commit_files`
reported success but the write did not persist. Re-ran the exact same `device_commit_files` call a
second time and this time verified it immediately by staging the file straight back
(`device_stage_files` on the same path) rather than trusting the tool's own success report — the
readback then showed the correct 6,218-byte content with a fresh timestamp. **This means the
user's rebuild after the first fix was always going to reproduce the bug unchanged, because it was
still compiling the unfixed project file** — not a sign the diagnosis or fix was wrong.

**Fix, now actually confirmed in place (verified by reading the file back from the device, not
just trusting the write's return value):** `Primal.csproj` has `<Compile Remove>` entries for
`Gimmicks\AltCharacterIdentity.cs`, `Characters\**\*.cs`, and `CharacterSelect\**\*.cs`. Confirmed
safe by grepping the whole tree for `AltRoster`, `AllCharacters`, and `CharacterSelect` outside
those two folders: the only hits were in `PrimalOptions.cs`, and those are a same-named but
unrelated local `CharacterEntry[]` array called `AllCharacters` — not a reference to
`Characters.AllCharacters` or the `CharacterSelect` namespace at all. Nothing in the current design
(real vanilla characters + gimmicks gated on `PrimalSlot.IsActive`) touches either dead folder.
Verified the edited `.csproj` is well-formed XML with
`python3 -c "import xml.etree.ElementTree as ET; ET.parse(...)"` before redelivering (see Gotcha
#5's XML pitfall — avoided `--` in the comment).

**Confirmed fixed (2026-09-11):** after this session copied the correct DLL directly into the
installed game folder (see below), the user relaunched and confirmed the character select screen
works. This gotcha is closed — the two originally-requested items (UI merge, Primal names) are what
still need checking, now that this unrelated interference is actually gone from the running game.

**Second "nothing changed" report, same session:** the user rebuilt and copied the file again and
reported the identical symptom. Re-checked real evidence rather than re-theorizing: `bin/Debug/
Primal.dll` had genuinely shrunk to 31,744 bytes (from 34,304) with a fresh
`CoreCompileInputs.cache` timestamp matching the build — proof the `.csproj` fix compiled correctly
this time. But the actual **installed** copy at `...\StreamingAssets\mods\Primal\plugins\
Primal.dll` was still 34,304 bytes at the old timestamp — the manual copy step into the game
folder hadn't happened. Re-read `exceptionLog.txt` fresh and it showed the byte-for-byte identical
`MethodAccessException` from `SlugcatSelectMenuHooks.Postfix`, confirming the game was still running
the old DLL. Since this session already had folder access to the Steam game root (granted earlier
this session, see above), copied the fixed `mod/plugins/Primal.dll` there directly with
`device_commit_files` instead of asking the user to redo the manual step, and verified the copy
landed (31,744 bytes, fresh timestamp) before saying anything. **The fix was correct and compiling
correctly the whole time; both "nothing changed" reports were deploy-step gaps, not logic bugs** —
first this session's own doc-push silently failing, then the user's manual game-folder copy step
being missed. Neither would have been caught without reading real file sizes/timestamps and the raw
exception log instead of trusting either side's "I did it" at face value.

**Lessons:**
- A "dead code, doesn't run anymore" file can still be very much alive as a compiled Harmony patch
  — the patch itself doesn't care whether the classes it cooperates with (here, `AltRoster`'s
  registry) ever actually get populated; it just runs every frame regardless and can fail in a very
  active, very visible way even though every human involved considers that whole design abandoned.
  When cleaning up a discarded design, check for *all* of its `[HarmonyPatch]` classes, not just the
  one that happens to be a compile error.
- **A `device_commit_files` (or similar) call reporting success is not proof the write landed.**
  After pushing a fix the user is about to rebuild against, read the file straight back from the
  device (stage it again, or `device_list_dir` its size/mtime) and confirm the change is actually
  there *before* telling the user to rebuild — especially after a "this did nothing" report, where
  a silently-failed sync is at least as likely as the fix itself being wrong. This is the same
  category of mistake as Gotcha #5 (trusting an artifact without checking it), just on the write
  side instead of the read side.

## Gotcha #7 — Remix's own config-load pass silently skips any OptionInterface with nothing Bound yet, so gimmick assignments never survived a game restart

**Reported by the user (2026-09-12): "on game reset, gimmicks dont get saved on which characters
they got applied to."** Not a save-file issue and not a Configurable/UI bug in the usual sense —
this mod's per-(character, gimmick) assignments are stored as ordinary Remix `Configurable<bool>`
values via `config.Bind(...)` inside `PrimalOptions.Initialize()`, which is the completely
standard pattern every Rain World mod uses for persistent settings. The bug was in *when*
`Initialize()` ran relative to when the game actually loads mods' saved config files off disk —
something this mod (like most mods) never had to think about before, because usually a player
opens Remix early enough that it doesn't matter. Root-caused by disassembling the real game
assembly's Remix/config plumbing end to end rather than guessing:

- `RainWorld.PostModsInit` (vanilla, runs automatically right after `OnModsInit` on every single
  game launch — no player action needed) calls `MachineConnector._LoadAllConfigs()`, which loops
  every registered `OptionInterface` and calls `_LoadConfigFile()` on each.
- `OptionInterface._LoadConfigFile()` is a **no-op unless `HasConfigurables()` is already true** —
  i.e. unless `config.Bind()` has already been called for at least one key on that OI. Only then
  does it call `ConfigHolder.Reload()`, the method that actually reads the mod's saved config file
  off disk (`ConfigHolder.GetConfigPath()`) and applies the saved values to the already-bound
  `Configurable`s.
- Separately, confirmed by disassembling `ConfigContainer._LoadItfs`/`_InitializeItf`: a
  newly-registered OI (ours, via `MachineConnector.SetRegisteredOI`) is just stored in the mod
  list's `OptItfs` array. **`Initialize()` is only ever called on whichever OI is currently
  "active"** — i.e. only once the player actually selects this mod's page in the in-game Mods
  menu. Nothing calls it automatically at game startup for an ordinary mod.
- Put together: before this fix, `config.Bind()` (the thing that makes `HasConfigurables()` true)
  only ever happened inside `Initialize()`, which only ever ran once the player opened this mod's
  Remix page. So on every single game launch, by the time `PostModsInit`'s automatic load pass
  ran, `HasConfigurables()` was still false, `_LoadConfigFile()` silently did nothing, and last
  session's saved (character, gimmick) assignments were never read off disk into memory for that
  game session at all — regardless of whether the player went on to open Remix *later* that
  session. Every gimmick's own Harmony patch calls `GimmickAssignments.IsEnabled()`, which falls
  back to the hardcoded `GimmickDefaults` whenever nothing has been `Register`ed for that key yet —
  so in practice this looked exactly like "gimmick assignments silently reset to their defaults
  every time the game restarts," matching the user's report precisely.
- **First fix (2026-09-12, round 1 — necessary but, per round 2 below, not sufficient on its
  own)**: split `PrimalOptions.Initialize()`'s config-binding loop out into its own
  idempotent method, `EnsureConfigBound()` (guarded by a `_configBound` flag, since
  `ConfigHolder.Bind()` throws `ArgumentException` if the same key is ever bound twice), and call
  it from **two** places: eagerly, from `RainWorldHooks.Postfix` right after
  `MachineConnector.SetRegisteredOI` (i.e. during `RainWorld.OnModsInit`, which always runs before
  `PostModsInit`) — making `HasConfigurables()` true in time for `PostModsInit`'s automatic load
  pass to find and load the saved file — and lazily, still inside `Initialize()` itself, unchanged
  in effect, so the interactive Remix-menu path keeps working exactly as before (the guard makes
  the second call a no-op when the eager call already ran). `Initialize()` otherwise still builds
  the actual UI (`OpTab`/labels/grid) exactly as before — only the config-binding loop moved, and
  confirmed via disassembly to be pure dictionary/string bookkeeping with no Unity/Futile
  dependency, so it's completely safe to run as early as `OnModsInit`, well before any menu/canvas
  exists.

**Round 2 (2026-09-12, same day) — the user tested this fix and reported it still didn't work**:
"dotnet build, copied files, added no passages to inv, reset game, no change. The game did not
however give me an option to apply the saved config of the remix settings." That last sentence was
the key clue that the round-1 analysis above had one claim wrong: **"saving was never the broken
half" was incorrect.** Re-disassembled `ModdingMenu.Singal()` with much wider context around its
`_SaveConfigFile()` call site and found it sits in a distinct branch, separate from the plain
"back out of this screen" branch (`cfgContainer._SwitchMode(1)` alone, no save) that sits right
next to it — meaning Remix's per-mod config screen chrome has its **own dedicated "SAVE" button**
(`ConfigConnector.MutualVerticalSaveButtonBind` is the actual UI wiring for it), separate from
simply backing out to the mods list, and a separate "RESET" button
(`ConfigContainer._ResetCurrentConfig`) right next to it. **Config was never being saved to disk
at all** unless the player specifically found and clicked that native SAVE button — which this
mod's own screen, filling virtually the entire ~600×600 area with its own permanent character
grid, gives no indication of and may well visually crowd out entirely. This matches the user's
report exactly: nothing was ever written to disk, so round 1's load-side fix had nothing to load.
- **Second fix (2026-09-12, round 2 — this is the one that actually closes the loop)**: rather
  than rely on the player ever finding/clicking that separate native button, `SetEnabledFor()` —
  the single method both `AddGimmick` and `RemoveGimmick` already funnel every assignment change
  through — now calls `config.Save()` (`ConfigHolder.Save()`, confirmed via disassembly to be a
  plain public method, the same one `_SaveConfigFile()` itself calls at its own tail end)
  immediately and unconditionally right after updating the `Configurable<bool>`'s `Value`. This
  makes saving fully self-contained: every assignment change is on disk the instant the player
  clicks +/x, with no separate button to find, and no window where a crash or alt-F4 could lose an
  unsaved change. Wrapped in a try/catch (logs and continues on failure, same defensive pattern as
  `RainWorldHooks`) since this now runs inside a UI click handler rather than a one-time startup
  hook. Pushed `PrimalOptions.cs` (30,933 bytes) and verified it landed correctly after one
  silent-write retry (`force: true`).
- **Not yet built or tested** as of this doc — next: `dotnet build`, copy the DLL, then the same
  end-to-end test as before but this time it should actually work: assign/unassign a few gimmicks
  on some character (no need to find any native SAVE button now), fully quit and relaunch the game
  **without opening Remix at all**, and confirm the assignments held (e.g. the alt-slot character
  shows the right "PRIMAL X" description, or a gimmick's own effect is present/absent as expected).

**Lessons:**
- A `Configurable<T>` bound via `config.Bind()` only reflects a previously-saved value once *both*
  halves of Remix's own plumbing have run for that key — `Bind()` itself, and
  `ConfigHolder.Reload()` (triggered automatically only for OIs that already have at least one
  bound key by the time `RainWorld.PostModsInit` fires, or manually whenever the player's own menu
  navigation calls it). Any mod value that needs to be correct on the very first frame of gameplay
  needs its `config.Bind()` calls to happen at `OnModsInit` time, not lazily inside `Initialize()`.
- **Don't assume a framework's standard UI affordance (here: a native "SAVE" button) is actually
  reachable once a mod has replaced that screen's content with its own custom layout.** The first
  round of this investigation assumed saving "already worked" because *some* code path in the
  vanilla assembly called `_SaveConfigFile()` — without checking whether that path was actually
  exposed/usable given this mod's own UI covering the same screen space. The user's exact wording
  ("didn't give me an option to... apply the saved config") was the tell that this assumption, not
  just the load-timing bug, needed re-checking — a lesson in reading a bug report's precise phrasing
  rather than pattern-matching it to the fix already in hand.
- When a reported-fixed bug comes back as "still doesn't work" after a real rebuild+redeploy+retest
  cycle (i.e. the deploy-gap explanation from Gotcha #5/#6 is ruled out), don't re-verify the same
  disassembly conclusion harder — go find the *other* code path adjacent to the one already traced
  (here, the sibling branches right next to the call site already found) before concluding the fix
  itself was implemented correctly but insufficient.

**Round 3 (2026-09-12, same day again) — round 2's fix also didn't work**: "still dont work man,
rebuild copy, open game, add no passage to inv, reopen, did not save." Round 2's own claim that
`ConfigHolder.Save()` is "a plain public method" turned out to be the mistake — it was inferred
from the compile-time reference assembly's flags rather than checked against the real runtime
assembly, exactly the trap Gotcha #1 already exists to warn about, just never applied to a *method*
call before this. Rather than re-disassemble the same plumbing a third time on theory alone, this
round was root-caused with two pieces of direct, concrete evidence instead:

1. **Checked the user's actual on-disk config folder.** Rain World's persistent data lives at
   Unity's `Application.persistentDataPath` for this game, which is
   `C:\Users\rokas\AppData\LocalLow\Videocult\Rain World\` (company "Videocult"), with per-mod Remix
   config files at `...\Rain World\ModConfigs\<mod-id>.txt`. This required requesting new device
   folder access (`C:\Users\rokas\AppData\LocalLow\Videocult`) since it hadn't been granted before —
   granted, then listed. Found 21 *other* mods' config files sitting right there, but **no
   `rokas.primal.txt` at all**. This is the single most useful fact of the whole
   investigation: it proves `config.Save()` was never succeeding even once, for any assignment,
   ever — not "saving but something else clobbers it," not "saving but to the wrong path," not an
   intermittent failure. Genuinely zero bytes ever written.
2. **Checked BepInEx's own log file directly** (`...\Rain World\BepInEx\LogOutput.log`, staged and
   grepped for "Primal") instead of guessing why. It had the exact answer, logged by round
   2's own try/catch:
   ```
   [Error  :AltCharacters] config.Save() threw while persisting a gimmick assignment (Inv/NoPassages -> True) -- the change is still in effect this session, but may not survive a restart: System.MethodAccessException: Method `OptionInterface+ConfigHolder.Save()' is inaccessible from method `AltCharacters.Gimmicks.AltCharactersOptions.SetEnabledFor(string,string,bool)'
   ```
   `ConfigHolder.Save()` shows `MethodAttributes` flags `0x86` (`Public | HideBySig`) in this
   project's compile-time reference assembly (`lib/PUBLIC-Assembly-CSharp.dll`) — but per Gotcha #1,
   that assembly is a "publicized" copy with every member forced public at compile time, and the
   real runtime assembly genuinely does *not* expose this method, so every single call threw
   `MethodAccessException` unconditionally. Round 2's own defensive try/catch was working exactly as
   designed the whole time (catching the exception, logging it, letting the session continue) — it
   just logged into a file nobody had checked yet.
- **Third fix (2026-09-12, round 3)**: apply the same reflection-based workaround already used for
  non-public *fields* elsewhere in this codebase (`NoPassagesGimmick.cs`,
  `PrimalCharacterNames.cs`) to this *method* instead. Added
  `private static readonly MethodInfo ConfigSaveMethod = AccessTools.Method(typeof(ConfigHolder), "Save");`
  as a static field on `PrimalOptions` (resolved once, not per-call), and changed
  `SetEnabledFor()`'s save call from `config.Save();` to `ConfigSaveMethod?.Invoke(config, null);` —
  `AccessTools.Method(...).Invoke(...)` bypasses the runtime accessibility check the exact same way
  `AccessTools.Field(...).GetValue()/SetValue()` already does for fields, so this should now
  actually reach `File.WriteAllText` inside the real `ConfigHolder.Save()` instead of throwing
  before ever getting there. Also corrected the two doc comments (on `EnsureConfigBound()` and
  `SetEnabledFor()`) that previously asserted saving "already worked" / `Save()` is "a plain public
  method" — both were wrong, now point at this section for the full trace. Pushed
  `PrimalOptions.cs` (33,612 bytes) and verified via `device_list_dir` that it landed
  correctly on the very first attempt (no silent-write retry needed this time).
- **Not yet built or tested** as of this doc — next: `dotnet build`, copy the DLL, then the same
  end-to-end test as rounds 1–2: assign/unassign a gimmick (e.g. add "No Passages" to Inv again),
  fully quit and relaunch the game **without opening Remix at all**, and this time also check that
  `...\Rain World\ModConfigs\rokas.primal.txt` actually exists and contains the assignment —
  don't rely on in-game behavior alone to confirm this, given how many rounds "should be fixed now"
  has already taken. If it still fails, check `LogOutput.log` again first, before any more
  disassembly guessing — that log is now a confirmed, fast way to get ground truth on any Remix/
  config-related failure rather than re-deriving one from IL.

**Additional lessons from round 3:**
- **Gotcha #1's "reference assembly is publicized, don't trust its access-flags" warning applies to
  *methods*, not just fields.** This codebase had already generalized the defensive
  `AccessTools.Field(...)` pattern to every menu-side field it touches, but kept calling
  `config.Save()` directly without applying the equivalent `AccessTools.Method(...)` treatment —
  worth scanning for anywhere else in this codebase that calls a method on a type from the
  publicized reference assembly (especially anything nested inside `OptionInterface`, `Menu.*`, or
  other framework/vanilla types this mod doesn't fully control) without going through reflection.
- **When a defensive try/catch is added specifically to survive an unknown failure mode, the
  logged exception is the fastest way to find that failure mode — check the log before
  re-theorizing.** Round 2 already had the exact answer sitting in `LogOutput.log` the whole time;
  it just hadn't been checked yet because round 3 started from "let's re-verify the fix is correct"
  rather than "let's read what our own error handler already caught."
- **Checking the actual on-disk artifact a fix is supposed to produce (here: the mod's own
  `ModConfigs\*.txt` file) is a strong, cheap sanity check before or alongside log-reading** — it
  can immediately distinguish "never wrote anything, ever" from subtler bugs like "wrote it to the
  wrong place" or "writes work but get overwritten/reset somewhere else," which would each need a
  completely different fix.

## Gotcha #8 — an OpScrollBox's own AddItems'd children are never cleaned up by the ordinary ShowDetail/RemoveItems teardown

**2026-09-15, later session (first one actually run on the user's own machine with a live game
window, not a container) — the user reported "you dont have a good grasp on the scaling of UI
elements — some take up significantly less space than they should, others far too much" and asked
for a better method of getting Remix UI sizing right.** Rather than keep guessing pixel numbers
blind, this session used `computer-use` to actually launch the game, open Primal's Remix screen live,
and click through it — the first time any session working on this mod could see the real screen
instead of reasoning from disassembly alone. That surfaced a real, reproducible bug within a few
minutes of clicking around, and it isn't a sizing/scaling issue at all: it's a stale-element leak.

**Symptom, live:** open a character's Gimmicks table, click "+" to open the add-gimmick picker
(`RenderAddGimmick`, an `OpScrollBox`), interact with it (scroll, or just let it render), then
navigate anywhere else (a different character in the always-visible grid, the "< CharacterName" back
button, a gimmick's settings view). The new view's own elements often don't respond to clicks at all,
or a click lands on and silently fires a completely different, invisible control left over from the
*previous* screen (confirmed directly: a click aimed at the "Monk" grid button instead silently fired
a stale "Kill List" row from an earlier add-gimmick picker visit, assigning Kill List to Gourmand —
not Monk — with nothing on screen indicating this happened).

**Root cause, confirmed by reading the decompiled framework source (`reference/GameSource/
Menu.Remix.MixedUI/OpScrollBox.cs` and `OpTab.cs`), not guessed:**
- `OpScrollBox.AddItems(items)` (used by both `RenderAddGimmick` for its rows and
  `RenderGimmickSettings` for a gimmick's settings controls) calls `tab.AddItems(uIelement)` on
  **each item directly** — so every row/control becomes the `OpTab`'s own direct child, exactly like
  any other element added straight through `_tab.AddItems(...)`. It also adds each one to the scroll
  box's own internal `items` set, for its own scrolling/draw logic.
- `PrimalOptions.ShowDetail(items)` only ever put the `OpScrollBox` *container* itself into
  `_detailItems` — never the individual rows/controls added to it afterward via `scrollBox.AddItems`
  (those didn't exist yet at the point `ShowDetail(items)` was called; per the framework's own
  two-step-commit requirement, the scroll box has to be attached to the tab before it can accept
  items at all — see "Add-gimmick picker made scrollable" below).
- `OpTab._RemoveItem` (decompiled) — what `_tab.RemoveItems(_detailItems.ToArray())` calls for each
  entry — detaches that one element from the tab's item list and container, but **never calls
  `Deactivate()` on it**. `OpScrollBox.Deactivate()` is what cascades into the scroll box's own
  children (`foreach (item in items) item.Deactivate();`) — and since removal never calls it, removing
  just the (by-then-empty-as-far-as-`_detailItems`-knows) scroll box leaves every row/control it ever
  held permanently attached to `_tab.items`, permanently undetached from `_container` (still drawn,
  invisibly overlapping whatever renders in that screen region afterward), and still wired to
  whichever `(characterID, gimmickID)` closure it captured when it was built.
- This means **every visit** to the add-gimmick picker, and **every visit** to any gimmick's settings
  sub-view (all of them go through this same `OpScrollBox` wrapper as of the 2026-09-15 scrollable-
  settings change, not just Chains — see "Per-gimmick settings sub-view made scrollable too" below),
  permanently leaks that view's controls. They accumulate for the rest of the `OptionInterface`
  instance's life (i.e., until the player fully closes and reopens the Remix screen), silently
  stacking up, still clickable, still capable of firing a stale assignment change with no visible sign
  anything happened.

**Fix, in `PrimalOptions.cs`:** both `RenderAddGimmick` and `RenderGimmickSettings` now also append
the scroll box's dynamically-built child elements into `_detailItems` (`_detailItems.AddRange(...)`),
right after handing them to `scrollBox.AddItems(...)` — no new API, just making sure the exact same
`_detailItems` bookkeeping this file already uses for every other element also covers these. The next
`ShowDetail` call's `_tab.RemoveItems(_detailItems.ToArray())` then genuinely detaches them along with
the scroll box container itself, instead of only ever removing the (by-then childless-as-far-as-the-
tab-knows) container.

**Confirmed fixed by rebuilding, redeploying, and relaunching the actual game** (this session had
direct machine access — no container limitation this time) — no more stray old rows/controls observed
after switching characters/views repeatedly post-fix, in the limited testing this session's
click-reliability issues (below) allowed.

**A live-testing caveat worth recording for next time:** synthetic mouse clicks against this game
window (via `computer-use`) were unreliable for anything beyond the first click on a freshly-opened
screen — hover/highlight state always updated correctly (proving click coordinates were accurate),
but the actual `OnClick` firing was intermittent, sometimes taking 2-3 identical repeated clicks on
the exact same element to register, and occasionally (the "x" remove button specifically, this
session) never firing at all despite many attempts. This looks like a timing mismatch between however
`computer-use` synthesizes a click (down+up) and Unity's own per-frame input polling, not a bug in
this mod — but it means **a real in-person test pass (the user actually at the keyboard) is still the
reliable way to confirm any Remix UI fix**, even now that this session can see the screen. Don't treat
a `computer-use` session's live click-through as equivalent to the user's own hands-on test.

**One side effect of this session's own testing, left behind:** while reproducing the bug, a stray
click landed on and fired a leftover "Kill List" row, assigning the Kill List gimmick to Gourmand for
real (confirmed persisted to `ModConfigs\rokas.primal.txt` — it survived a full game restart). Several
follow-up attempts to click its own "x" to remove it did not register (see the click-reliability
caveat above). **Left in place rather than force it via `RESET CONFIG`**, since that button resets
*every* character's assignments back to defaults, which would blow away any real configuration the
user has made independently of this session. If Gourmand still shows Kill List assigned and that
wasn't wanted, remove it by hand (one click on its "x") next time the Remix screen is open.

**Methodology takeaway, answering "how do I get UI scaling right" directly:** the fix here was found
by *reproducing the actual behavior live* and then reading the exact framework source that explains
it — not by re-guessing pixel coordinates. Now that this project can run on a machine with both the
game and screen-automation access, that's the reliable path for any future "layout looks wrong"
report: (1) get it open on screen (this session's own click-reliability issues aside, the user's own
hands are always reliable for this), (2) if something behaves oddly, don't assume it's a `pos`/`size`
number that needs nudging — check whether it's actually a *stale/leftover element* problem first
(anything that goes through `OpScrollBox`, or any other framework object with its own child-tracking,
is a candidate), by reading that framework class's decompiled source in `reference/GameSource/` for
what its own `AddItems`/removal really does, the same way this bug was actually found. Only reach for
adjusting a literal coordinate/size constant once stale elements are ruled out.

## Misery gimmick, in detail (traced from a real reference mod — read before editing)

The user reported the original Misery implementation (a simple `aerobicLevel` threshold override on
`Player.ClassMechanicsGourmand`) as "significantly weaker" than a specific comparison point: the
"Misery Mode" toggle in the Steam Workshop mod **"Gourmand Stamina Display"** (author "dannad",
workshop item ID **2980291710**, currently installed at
`C:\Program Files (x86)\Steam\steamapps\workshop\content\312520\2980291710\`).

That mod ships compiled-only (`plugins\RW_Gourmand_Stamina.dll`, no public source found). It was
decompiled in-session with `ikdasm` (IL disassembler, already present on this container — no
network/NuGet access is available in this environment, `dotnet tool install ilspycmd` fails, proxy
blocks `api.nuget.org`) to find out what its "Misery Mode" checkbox — internally a
`Configurable<bool>` field literally named `permaStarve` — actually does. Findings:

- It hooks `Player.Jump()` and `Player.WallJump(int)` via MonoMod's `On.Player` (full copy-pasted
  reimplementations of both vanilla methods, since that's the only way that mod could intercept
  them). At the top of each, unconditionally: `self.gourmandExhausted = true; self.aerobicLevel =
  1f;` — i.e. **every jump or wall-jump instantly maxes out exhaustion**. There's no separate decay
  logic added; vanilla's own normal per-tick recovery elsewhere in `Player.Update` is what lets it
  drain back down between jumps. Net effect: jumping is very costly, and you have to wait it out.
- It also ORs `permaStarve.Value` into every one of those methods' own `self.PainJumps()` checks,
  so jumps also come out at vanilla's reduced "PainJumps" height/distance. `Player.PainJumps` is a
  real but narrow vanilla mechanic (see `Player.get_PainJumps`) normally gated on an unrelated,
  specific case (`SlugCatClass == Sofanthiel`, region "CC", not in a gate/shelter) — the reference
  mod is just reusing that existing "make jumps weak" branch as an implementation shortcut, not
  because it's semantically about stamina.

This mod's `MiseryGimmick.cs` now reproduces both effects via two small, surgical Harmony patches
instead of reimplementing whole methods:
- `Prefix` on `Player.Jump()` and `Player.WallJump(int)` — force `gourmandExhausted`/`aerobicLevel`
  right before the original runs, exactly where/when the reference mod does it.
- `Postfix` on the `Player.PainJumps` property getter itself (broader than the reference mod could
  manage — it applies everywhere vanilla reads the property, not just those two call sites).

All three patches gate on `MiseryEffect.IsActiveFor(player)`, which checks `PrimalSlot.IsActive` +
`GimmickAssignments.IsEnabled(characterID, "Misery")`, same pattern as every other gimmick.

**Not yet re-tested by the user as of this doc** — the rebuilt DLL and updated `.cs` files were
deployed, but no test result has come back yet. If reported as still "too weak" or "too strong",
re-open `/tmp` on a future container's `ikdasm` disassembly workflow (see the workflow described
above — re-locate the mod's workshop ID via its `modinfo.json` `"name"` field if this recipe needs
repeating for another reference mod) rather than guessing.

## TimeLimit and NoSpears gimmicks, in detail (new, both untested as of this doc)

Requested by the user as two more Spearmaster-specific gimmicks, both now default-assigned to
Spearmaster alongside his existing OneSpear:

- **TimeLimit** (`TimeLimitGimmick.cs`): "if the in-game timer has passed an hour, the character
  will die. Later on we can implement some sort of game over screen, for now if timer > 1hour = kill
  character that tick." Implemented as a Postfix on `Player.Update(bool)` that kills the player
  (`self.Die()`) the instant `self.room.game.clock` (the current playthrough's own tick counter,
  40 ticks/second, never reset except by a fresh game/continue) passes 144000 ticks. **No game-over
  screen exists yet** — that's explicitly a later step per the user's own framing, don't add one
  unprompted. A `ConditionalWeakTable<Player, object>` guards against calling `Die()` more than
  once per player once the clock has passed the threshold (it keeps counting up afterward).
- **NoSpears** (`NoSpearsGimmick.cs`): "you cant pick up spears - they become non grabbable objects
  if the applied character has the gimmick." Implemented as a Postfix on
  `Player.Grabability(PhysicalObject)` — forces the result to `Player.ObjectGrabability.CantGrab`
  whenever the object `is Spear` and the specific player asking has the gimmick. Because
  `Grabability` is a per-player method (not a property on the object), this only affects the one
  assigned character — everyone else can still pick up spears normally. Verified this does not
  fight with OneSpear's own tail-pull spear, which is attached via `Player.SlugcatGrab(...)`, a
  separate vanilla method that bypasses `Grabability` entirely (see NoSpearsGimmick.cs's own doc
  comment).

Both gate the usual way (`PrimalSlot.IsActive` + `GimmickAssignments.IsEnabled`). Neither has been
tested in-game yet — next session should ask the user to confirm: (1) Spearmaster actually dies
at the 1-hour mark (can't feasibly wait a full hour to verify without the user doing so, but if
reported as not firing, the first thing to check is whether `self.room` is non-null at the point
`Player.Update` runs for a freshly spawned Spearmaster — if this fires before the character is
fully in a room the null-guard would just skip silently every tick, which would look like "never
happens" rather than an exception), and (2) spears genuinely can't be picked up while the gimmick
is on, without breaking the existing one-time tail-pull.

## NoYeeks, NoCicadas, and NoMauling gimmicks, in detail (new, all three untested as of this doc)

Requested by the user as "a couple more gimmicks: no grabbing yeeks, no grabbing squidcadas and
no mauling." All three default-assigned to Artificer (an assumption, not explicitly specified —
see the doc comment in `GimmickDefaults.cs` for why; easily reassigned in the Remix menu if wrong).
**Update (2026-09-12): also default-assigned to Inv** (`MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel`
— see "Config menu UI, in detail" for how Inv was added to the character roster), along with
NoSpears, per explicit request. Confirmed via disassembly that this isn't just thematic for
NoMauling specifically — `SlugcatStats.SlugcatCanMaul` already allows *both* Artificer and
Sofanthiel in vanilla, so NoMauling has a genuine effect on Inv too, not just Artificer.

- **NoYeeks** (`NoYeeksGimmick.cs`): blocks picking up `MoreSlugcats.Yeek` (Watcher's small
  mask-wearing creature) the same way NoSpears blocks spears — a Postfix on
  `Player.Grabability(PhysicalObject)` forcing `Player.ObjectGrabability.CantGrab` when
  `obj is MoreSlugcats.Yeek` and the asking player has the gimmick. Confirmed via the real game
  assembly's TypeDef table that `Yeek` lives in the `MoreSlugcats` namespace (not root, unlike most
  of this file's other creature checks) and extends `AirBreatherCreature`. No settings — only one
  Yeek class exists, so there's nothing to sub-select the way NoSpears sub-selects spear types.
- **NoCicadas** (`NoCicadasGimmick.cs`), shown in the UI as "No Squidcadas": same mechanism, for
  root-namespace `Cicada` (extends `InsectoidCreature`). Confirmed via TypeDef that `CicadaA`/
  `CicadaB` (the two names this file's ChainsGimmick section already uses for its
  `excludeSquids`/preset dictionary) are just the two `CreatureTemplate.Type` enum variants of this
  one C# class, not separate types — so `obj is Cicada` alone covers both, unlike Spear's three
  genuinely distinct subclasses. No settings, same reasoning as NoYeeks.
- **NoMauling** (`NoMaulingGimmick.cs`): disables Artificer's own instant-kill "maul" grab. Traced
  by disassembling `Player.CanMaulCreature(Creature)` in the real game assembly (method body
  confirmed via the IL, not guessed): it already opens by checking
  `SlugcatStats.SlugcatCanMaul(this.SlugCatClass)` and returns `false` immediately if that's false,
  before any of its other checks (dead/stunned/friendly-fire/food-full/
  `IsCreatureLegalToHoldWithoutStun`) even run — so mauling is already exclusively an Artificer
  ability in vanilla, and this gimmick only ever has an effect on a character currently playing
  under Artificer's own `SlugCatClass`. Implemented as a Postfix on `CanMaulCreature` forcing
  `__result = false` whenever the gimmick is active and vanilla hadn't already said no. No
  settings — plain on/off.

All three follow the exact same gating pattern as every other per-player gimmick in this file
(`PrimalSlot.IsActive(rainWorld)` + `GimmickAssignments.IsEnabled(characterID, ID)`), are registered
in `GimmickRegistry.All` (confirmed `PrimalOptions.cs` still builds every UI loop from that
list dynamically, so no other file needed editing for these three to appear), and default-assigned
to Artificer in `GimmickDefaults.cs`. **Not yet built or tested in-game** — next: `dotnet build`,
and if clean, confirm in-game that (1) Yeeks and cicadas genuinely can't be picked up by
(Primal) Artificer while their gimmicks are on, and (2) the maul prompt/animation no longer
triggers on any creature for (Primal) Artificer while NoMauling is on.

## NoPassages gimmick, in detail (new, untested as of this doc)

Requested by the user as a separate ask in the same message that added Inv: "removing passages —
show them in the cycle completion/death menus, but disable the actual button to use them." No
default assignment — not tied to a specific character in the request.

Traced by disassembling `Menu.SleepAndDeathScreen.AddPassageButton(bool)` in the real game
assembly: it builds a single `Menu.SimpleButton` (guarded by "if the field is already set, return"
— so the actual construction only ever runs once per screen) and stores it in the screen's own
`passageButton` field. Every `Menu.ButtonTemplate` (which `SimpleButton` extends) carries a
`Menu.ButtonBehavior` (`buttonBehav`), and that class's own `Update()` (also disassembled) skips
its entire click-detection block — the part that ultimately calls `owner.Clicked()` — whenever
`buttonBehav.greyedOut` is true, while every other part of `Update` (the pulsing/highlight
animation) still runs normally. That's the exact "visible but inert" mechanism vanilla itself
already uses for menu options that exist but currently can't be chosen, so `NoPassagesGimmick.cs`
Postfixes `AddPassageButton` and sets that same flag true, rather than hiding the button or
inventing a new disabled-visual style. The button still renders and its tooltip/description still
work; only the click handler is skipped.

Determining *which character's save* a `SleepAndDeathScreen` belongs to (there's no `Player`
instance handy on a menu screen) goes through `KarmaLadderScreen.saveState.saveStateNumber` (a
`SlugcatStats.Name`) — `SleepAndDeathScreen` extends `KarmaLadderScreen`, which is where
`saveState` actually lives.

Accessibility note, same class of caution as Gotcha #1: `saveState`, `passageButton`,
`buttonBehav`, and `greyedOut` all show `public` in this project's compile-time (publicized)
reference assembly, but none of them have ever been touched by this mod before, so all four are
read/written via `AccessTools.Field` reflection rather than trusted directly — the same defensive
choice `PrimalCharacterNames.cs` already made for `SlugcatPageNewGame`'s labels.
`manager`/`rainWorld`, by contrast, are accessed directly (`__instance.manager?.rainWorld`) — there
is already in-mod precedent that those two are genuinely public (`PrimalCharacterNameHook` does the
same thing with `menu.manager.rainWorld` without issue).

Deliberately does **not** touch `AddExpeditionPassageButton` — the user asked about the cycle-
complete/death screen specifically, and Expedition mode is a separate game mode whose passage
handling is already independent (`AddPassageButton` itself no-ops when `rainWorld.ExpeditionMode`
is true, confirmed in its own disassembly).

**Not yet built or tested in-game** — next: `dotnet build`, and if clean, assign it to some
character via the Remix menu and confirm in-game that (1) the Passage button still appears and
still shows its normal tooltip on the cycle-complete/death screen, and (2) clicking it does
nothing (no sound, no passage-selection screen opens) while the gimmick is on.

## Chains gimmick, in detail (core mechanic confirmed in-game; settings import untested)

Requested by the user as "find the artificer chains mod and turn that into a gimmick." No mod
literally titled "Artificer Chains" exists on the Steam Workshop — two rounds of web search, and a
full cross-reference against the user's actual 30 subscribed Workshop items (built by staging and
reading every one of their `modinfo.json` files' `"name"` field, after `device_request_folder_access`
for the Workshop content folder), both came up empty. Asked the user directly; they clarified it's a
**local, unlisted mod** — found at
`RainWorld_Data\StreamingAssets\mods\artificerhell\plugins\artificer_rubicon_hell_bonanza.dll`
(mod id `"lockeveryroom"`, display name `"Artificer Rubicon Hell Bonanza (STUPID HALF BAKED
VERSION)"`, author `dannad`) on the user's own machine, loaded via BepInEx's `MultiFolderLoader`
(not `BepInEx\plugins\` — this game install keeps individual local/unlisted mods each in their own
folder under `RainWorld_Data\StreamingAssets\mods\`, same place vanilla's own `moreslugcats`,
`watcher`, etc. "mods" live, alongside `Primal` itself).

**Reverse-engineering.** Same no-decompiler workflow as everything else in this doc (see "Container
environment notes"): staged the DLL from the user's machine, disassembled every method on its three
real classes with `dnmeta.py` — `Artificer_Hell` (the top-level BepInEx plugin, 9 Harmony/MonoMod
hooks), `CustomHRGuardManager` (the actual lock/unlock state machine), and `MultiChain` (the chain
visual) — by reading their TypeDef/MethodDef/FieldDef tables and disassembling each method body.
Cross-referenced every vanilla member the mod touches against `PUBLIC-Assembly-CSharp.dll`'s own
metadata (field/method accessibility flags, and the `NestedClass` table for anything that might be a
nested type) before writing any C#, exactly the discipline Gotcha #1/#3 already established — this
paid off twice: `ShortcutData.Type` and `CreatureTemplate.Type` are both nested inside their parent
class (same class of gotcha as `Player.NPCStats` in the Slugpup gimmick, caught up front this time
instead of by a failed build), and `MoreSlugcatsEnums.MSCSoundID` (used for the lock/break sound
effects) is itself nested inside `MoreSlugcats.MoreSlugcatsEnums`. Unlike the Slugpup stats trace,
**almost everything this mod touches turned out to be genuinely public** in the real game assembly
(verified field-by-field, not assumed) — `Room.shortcuts`, `AbstractRoom.creatures`,
`MoreSlugcats.VoidChain` and its constructor, `AbstractCreatureAI`, `Creature.enteringShortCut` — so
`ChainsGimmick.cs` uses ordinary direct C# member access throughout, no `AccessTools` reflection
needed anywhere (a first for this codebase's gimmicks). One genuine surprise the metadata caught:
the mod's EelLizard banlist entry is `DLCSharedEnums.CreatureTemplateType.EelLizard`, not
`MoreSlugcatsEnums`'s own same-named `CreatureTemplateType` holder class — three separate DLC
"CreatureTemplateType" holder classes exist in the assembly (one per DLC), each just a static
container of `CreatureTemplate.Type`-typed fields, and guessing the wrong one would have been an
easy, silent mistake (both compile fine; only one is the actual value the source mod bans).

**What it actually does, mechanically (`CustomHRGuardManager`/`MultiChain`, faithfully ported):** on
entering a room, collect every living, non-in-shortcut, non-banned creature present
(`GetCreatureList`). If there are any: flip every `RoomExit`-type shortcut in the room to `DeadEnd`
(remembering which indices, to restore later), attach a `VoidChain`-based chain visual
(`MultiChain`) to up to 3 of those creatures — one glowing chain per sealed exit, each running from
that exit's entrance point to the creature's current position, animated with a slow sine-wave pulse
— play a lock sound, and redirect ("confuse") any of the tracked creatures currently pathing to
leave the room, or mid-flight into a shortcut back toward their own den. Every subsequent tick while
locked: forcibly cancel the *player's own* attempt to walk into any shortcut (shoving them back into
the room with velocity in the anti-shortcut direction) — flipping a shortcut's type alone stops it
functioning as a room connection, but does **not** stop the player's body from physically entering
the hole trigger first, so this check is load-bearing, ported as-is, not decorative; forbid every
tracked creature from retreating to its own den and cancel any shortcut entry it's mid-way through
(same reasoning: otherwise a creature just walks into a burrow and vanishes somewhere this gimmick
can no longer see, making the room permanently uncompletable); purge dead (or forced-dead, if it
reached its den anyway) creatures from the list, reassigning chains to keep up to 3 of the survivors
visibly chained. Once the list is empty (or the escape timer elapses), unlock: restore every sealed
shortcut to `RoomExit`, refresh the room's shortcut graphics (`ShortcutGraphics.NewRoom()`), dispose
every chain visual, play an unlock sound.

**What was deliberately NOT ported** (see `ChainsGimmick.cs`'s own header comment for the full
reasoning on each): `VultureAI.CreatureSpotted` — the source mod's *other* creature-arrival path,
which lets an already-circling vulture that merely *spots* the player (without itself entering the
room) start or join a lock; vultures/king vultures stay excluded from the arrival path below for
exactly this reason, so behavior stays internally consistent (a vulture only ever gets chained if
it was already present when the room was first evaluated). The source mod's four Remix-configurable
banlist presets and separate exclude-vultures/exclude-squids toggles are collapsed into one fixed
set here, modeled on its safest ("NoCritters") preset; its separate "huntedMode" option (forces
every creature to instantly notice the player) is a related-but-distinct feature, skipped. **This
port's own addition, not in the source mod:** a configurable per-character escape-timer safety net
(`ChainsSettings`, same file — seconds, 0 disables it, default 90s), added specifically because
this is fresh, never-in-game-tested code and a bug in the creature/chain bookkeeping turning into a
genuine permanent soft-lock (sealed exits that never reopen) is a much worse failure mode than
occasionally letting the player off easy before a room is fully cleared. The source mod's own
equivalent option defaults to disabled (no time-based way out at all) — deliberately not matched
here, for that reason.

**2026-09-12 addition: creatures arriving after the room is already being tracked.** First-round
testing (see "Outstanding items" below) confirmed the core lock/unlock mechanic works, but surfaced
a real gap: a room that was empty (or already cleared) *when entered* never caught a creature that
wandered in afterward while the player was still there — the arrival simply wasn't noticed at all.
The user asked for the source mod's own fix to be reused rather than reinvented ("the local
artificer hell mod had a lot of development to fix a lot of bugs"), so `Artificer_Hell
.Creature_NewRoom` (the hook that reacts to *any* creature finishing a room transition) and
`CustomHRGuardManager.AddNewCreature` (what it calls) were re-disassembled and ported faithfully —
`ChainsCreatureArrivalHook` (a new `[HarmonyPatch(typeof(Creature), nameof(Creature.NewRoom))]`
class) forwards every non-Slugcat arrival to `ChainsGimmick.HandleCreatureArrival` for each player
in the room with Chains enabled, which hands it to the new `TryAddArrivingCreature` — same guards
as the source (dead/already-tracked/Vulture-or-KingVulture/banned-type exclusion, a friendly-Lizard
exclusion via `friendTracker.friend == self`, a distinctly higher-pitched arrival chime), and if the
room wasn't already triggered, this arrival is what starts the lock now (`LockNow`, factored out of
`TryLockRoom` so both paths share the exact same seal/chain/sound/confuse sequence). One genuine bug
in the source mod's `AddNewCreature`-adjacent `GetCreatureList` was found and deliberately **not**
reproduced: repeated calls never `.Clear()` the list first, so they'd silently duplicate
already-tracked entries — this port's own list additions are always `Contains`-guarded instead, so
a creature can never be double-counted. The source mod's own `genocideMode` Remix toggle, which
gates this whole feature there, is now ported too (see the settings-import paragraph below) rather
than permanently collapsed into always-on behavior. A player who is themselves mid-shortcut when a
creature arrives has the arrival buffered (`ChainState.CreaturesToAdd`, ported from
`CustomHRGuardManager.CreaturesToAdd`) and flushed the next tick they aren't
(`FlushArrivalBuffer`), exactly matching the source mod's own `Player_NewRoom`-driven buffer flush.
**Confirmed working in-game by the user (2026-09-12)**, alongside the original lock/unlock
mechanic — the user reported "it looks like its implemented properly now" after this round of
testing.

**2026-09-12 settings import.** Once both of the above were confirmed working, the user asked to
import the rest of the source mod's own Remix settings screen too, specifically calling out its
creature-allowlist selection: "the local mod had more config stuff, such as creature allowlist
selection. Import all of these settings into the gimmick settings." Disassembled
`Artificer_HellOptions` (the source's own `OptionInterface` subclass, its `.ctor`/`Initialize`)
directly — a class not previously examined in detail, since earlier passes only needed the two
already-ported values it fed into `CustomHRGuardManager`. Its full option surface: `timer` (already
covered by this port's own escape timer, just with 0 instead of -1 as the "off" value); `logging`
(the source's own internal debug-log verbosity switch — **not** ported, since no other gimmick here
exposes a per-gimmick logging toggle and `Plugin.Log` already runs unconditionally everywhere else);
`genocideMode` (now `ChainsSettings.CatchLateArrivals`, gating the arrival-handling feature above —
source default is off, this port defaults it **on**, since that's now the confirmed, expected
behavor, not an opt-in extra); `selectedBanlist` (one of four named presets — "Funny" bans nothing,
"Hardcore" adds `GarbageWorm`/`VultureGrub`/`EelLizard`, "Casual" adds `TempleGuard`/`BigEel`/`Deer`
on top of Hardcore, "NoCritters" adds `Overseer`/`Spider`/`Leech`/`Fly` on top of Casual — recovered
by disassembling `CustomHRGuardManager`'s own constructor, which builds exactly this
`Dictionary<string, HashSet<CreatureTemplate.Type>>`) plus `excludeVultures`/`excludeSquids` (both
default **true** in the source — union in `Vulture`+`KingVulture`, or `CicadaA`+`CicadaB`,
regardless of preset; "squids" turned out to be community slang for cicadas once traced through the
metadata — there's no vanilla-named squid creature). The four named presets plus two exclude-toggles
were collapsed into one independent checkbox per creature type (`ChainsSettings.BanOptions`) rather
than reproduced as a preset dropdown/listbox — matching this codebase's own established pattern for
"which of several options are allowed" (`NoSpearsSettings`' one-checkbox-per-spear-type, same file
style), strictly more expressive than four fixed combinations, and avoiding a UI element (`OpListBox`
/ mutual-exclusion radio buttons) this codebase has never used and this container has no way to test
live. Every checkbox's default reproduces the source's own real defaults (Hardcore + both exclude
toggles) exactly — correcting this port's own earlier (pre-settings-pass) assumption that "NoCritters"
was a reasonable stand-in default; it wasn't actually the source's default at all. `huntedMode`
("Faux-hunted burden:") is now `ChainsSettings.FauxHuntedBurden`, wired to a **new**
`ChainsHuntedModeHook` Harmony patch on `ArtificialIntelligence.Update` — confirmed via disassembly
of the source's own `On.ArtificialIntelligence.Update` hook to be a genuinely global effect, not
scoped to a currently-locked room at all: every tick, for every creature's AI, if any player sharing
that creature's room has this checkbox on, the creature is forced to instantly notice that player
(`Tracker.SeeCreature`), bypassing stealth entirely (`VultureAI` skipped, unexplained in the source
but kept as-is). Off by default, matching the source. All four new settings live in `ChainsGimmick.cs`
alongside the existing `ChainsSettings.GetEscapeTimerSeconds`; `ChainsGimmick.BannedTypes` (the old
fixed `HashSet`) was removed entirely in favor of `ChainsSettings.IsBanned(characterID, type)`, now
checked identically by both the initial room scan (`GetCreatureList`) and the arrival path
(`TryAddArrivingCreature`) — previously those two used different exclusion logic (a fixed set for
one, a hardcoded Vulture/KingVulture-only check for the other), which this pass also quietly fixed
into one consistent, per-character-configurable rule.

**First build attempt failed** (`dotnet build`, 2026-09-12): `CS0029`/`CS0019` in the new
`ChainsHuntedModeHook`, both from the same root cause -- `AbstractWorldEntity.Room` (capital R,
used via `AbstractCreature owner.Room`) returns an `AbstractRoom`, not the realized `Room` this
method actually needed to compare against another creature's room. A genuine miss: everywhere else
in this file already correctly uses a realized `Creature`'s own `.room` field (lowercase, e.g.
`creature.room != room` in `TryAddArrivingCreature`) for exactly this kind of comparison, but this
new method reached for the wrong one of the two. **Fixed** by rerouting through
`owner.realizedCreature?.room` (and the equivalent `player.room` for the loop's own player check)
instead of `owner.Room`/`abstractPlayer.Room` -- no metadata surprise here, just a same-named
property/field pair on two different types (`AbstractWorldEntity.Room` vs. `Creature.room`) that
this pass conflated; worth remembering as a fourth "these look interchangeable but aren't" trap
alongside this project's existing Gotchas. Pushed the fix; user's next `dotnet build` **succeeded**
(`bin/Debug/Primal.dll` 48,640 bytes) and they copied it to the installed game folder
themselves — confirmed in sync (48,640 bytes, matching timestamp) via `device_list_dir` on both
paths.

**2026-09-12: quick-preset select menu brought back.** The original settings-import pass (above)
deliberately replaced the source's four-named-preset `OpListBox` with pure per-type checkboxes,
reasoning that this codebase had never used `OpListBox` and there was no way to verify its
constructor from this container. The user asked for the select menu back regardless. This time,
rather than guess, disassembled `Menu.Remix.MixedUI.OpListBox` directly from
`lib/PUBLIC-Assembly-CSharp.dll` (the real game assembly, not the source mod's DLL) to get its
actual constructors from metadata: four overloads exist, and the one used
(`OpListBox(Configurable<string> config, Vector2 pos, float width, string[] array, int lineCount,
bool downward)`) takes its option strings directly as a plain `string[]` rather than needing a
`List<ListItem>` or relying on the bound `Configurable`'s own acceptable-range object (the source
mod's own call site uses a different, shorter overload that has no explicit item source at all,
implying `OpListBox` can also pull items from the config's own acceptable-range when one isn't
passed directly — not used here since the explicit-array overload was simpler to verify was
correct). Also found `Configurable<T>.OnChange` (public `add_OnChange`/`remove_OnChange` -- a
parameterless `OnEventHandler` delegate), used to react to a preset pick.

Implementation (`ChainsSettings`): a new `Configurable<string> _banlistPreset` per character
(default `"Hardcore"`, matching the source), rendered via that `OpListBox` next to a "Quick preset:"
label. Selecting one runs `ApplyPreset`, which sets the 10 non-grouped `BanOptions` checkboxes
(everything except "Vultures"/"Cicadas") to whatever `PresetContents[presetName]` lists, leaving the
two grouped checkboxes untouched — deliberately mirroring the source's own `selectedBanlist` vs.
`excludeVultures`/`excludeSquids` independence (see the original settings-import paragraph above for
the exact preset contents). Individual checkboxes can still be changed by hand afterward; the preset
picker is a one-shot convenience action, not an enforced constraint, so it never fights a later
manual checkbox edit. `BuildUI`'s layout shifted down to make room (~140px added below the two
existing checkboxes) — the exact vertical fit for the listbox itself is estimated, not confirmed
against a live screenshot, so watch for clipping against the drill-down's ~600×600 bound when this
is finally tested.

**Second build attempt also failed** (`dotnet build`, 2026-09-12): `CS1503` x2 on the `OpListBox`
call -- "cannot convert `string[]` to `List<ListItem>`" and "cannot convert `int` to `ushort`".
Root cause was a genuine mistake in the *first* hand-decode of `OpListBox`'s signature: byte `0x07`
was read as `ELEMENT_TYPE_I4` (int) when it's actually `ELEMENT_TYPE_U2` (ushort) -- a one-off
transcription slip in the ECMA-335 element-type table, not a metadata surprise. Because the call
passed a plain `int` (`PresetNames.Length`) where the real overload needs `ushort`, C#'s overload
resolution couldn't match the intended `string[]`-taking constructor at all and fell back to
reporting a mismatch against a *different* overload (the one taking `List<ListItem>`) instead --
which is why the error pointed at the wrong parameter type entirely and took a moment to place.
**Fixed** by casting to `(ushort)PresetNames.Length` at the call site, and by writing a proper
from-scratch ECMA-335 signature decoder (`scratchpad/dnparse/sigdecode.py`, not committed to the
mod's own source tree -- it's a reverse-engineering tool for this container, not part of
`Primal` itself) to re-verify the whole signature byte-by-byte afterward, rather than
continuing to eyeball hex tables by hand -- worth reaching for on any future disassembly work in
this codebase to avoid repeating this exact class of mistake. Pushed the fix
(`ChainsGimmick.cs` now 61,173 bytes, verified).

**Also noticed while re-staging before this push:** the user has directly hand-edited
`PrimalCharacterNames.cs` on their own machine since it was last pushed -- Artificer's description
intro line now reads "A violent combatant, master of explosives, cursed to seek out any prey in
sight. A journey of endless bloodshed and warfare." (closer to vanilla Artificer's own flavor than
this session's original generic "changed by something primal" line). **Left as-is, not
overwritten** -- future pushes to this file should re-stage and re-read it first to avoid clobbering
further manual edits, the same discipline already applied to every device-side write in this doc.

Registered in `GimmickRegistry.All` and given one default assignment in `GimmickDefaults.cs`:
Artificer (matching the user's own stated use case — they play the source mod on Artificer in a
Rubicon run). **As of this doc: the core mechanic is confirmed working in-game; the settings-import
and quick-preset-select-menu additions are pushed and now believed to compile cleanly (two rounds of
build errors found and fixed), but neither has actually been rebuilt/confirmed yet** — next: rebuild,
redeploy, open Chains' settings for a character, and check the full control
set renders without clipping or throwing.

**2026-09-13: pipe-type shortcuts were never actually sealed.** The user reported the real gap
directly: "you didnt fully import artificerhell chain logic because it excludes shortcuts, the
rooms internal pipes marked by triangles, and allows going through them, while pipes, room to room
connections, should remain chained." Re-disassembled the source mod's own `CustomHRGuardManager
.LockPlayer` to check whether *it* handled this and this port had simply missed something in
translation — it doesn't: the source mod's own seal loop also only ever checks
`shortCutType == Type.RoomExit`, byte-for-byte the same condition this port already had. So this
wasn't a porting gap relative to the source at all; it's a real gameplay gap the source mod itself
also has, which the user's own report was actually pointing at.

Dumped `ShortcutData`'s nested `Type` enum directly from the game's own metadata (Field/Constant
tables in `lib/PUBLIC-Assembly-CSharp.dll`, not guessed or recalled from memory) to get its complete,
real member list: **Normal, RoomExit, CreatureHole, NPCTransportation, RegionTransportation,
DeadEnd.** Confirmed `Room.ShorcutEntranceHoleDirection` — the method both `UpdateLock`'s
belt-and-suspenders shove and `MultiChain`'s own chain-anchor placement already depend on — is
purely geometric (four-direction wall/terrain probing around a tile; disassembled its full body and
it never reads `ShortcutData.Type` at all), so extending which types get sealed can't break either
of those call sites.

**First fix attempt (same day, superseded below) sealed the wrong pair.** Reasoning from the
*names* alone, the first pass sealed `RoomExit` + `NPCTransportation` + `RegionTransportation`
(treating both "transportation" types as the long-distance pipe systems) and left `CreatureHole`
unsealed (reasoning it was redundant with `UpdateLock`'s existing per-tick den-avoidance logic). The
user corrected this directly, giving the actual intended behavior explicitly rather than leaving it
to inference:

```
room to room PIPES        - locked
within room SHORTCUTS     - unlocked
creature DENS             - locked
within room TRANSPORT for creatures - unlocked
```

Re-checked by grepping the source mod's **entire** disassembly (every method on
`Artificer_Hell`/`CustomHRGuardManager`/`MultiChain`, not just `LockPlayer`) for every
`ShortcutData.Type` reference it makes, anywhere: it only ever touches `RoomExit` and `DeadEnd`.
Worth being explicit about: **the source mod's own compiled code never implements this fine-grained
per-type distinction at all** — the mapping above is not something recovered from the reference
mod's own logic, it's the user's own domain-knowledge correction of a gap that exists in the source
mod too. The first attempt's names-based guess about `NPCTransportation` was simply wrong: despite
the "transportation" name, `NPCTransportation` is Rain World's *within-room* creature-transport
shortcut (an AI/pathing convenience, never leaves the room) — the "within room TRANSPORT for
creatures" the user says should stay unlocked. `CreatureHole` is a real den entrance a creature can
disappear through — the "creature DENS" the user says should be locked — and relying solely on
`UpdateLock`'s per-tick `doesNotUseDens`/`enteringShortCut` guarding only protects creatures already
in `state.CreatureList`, not the room as a whole (an untracked creature wandering in, or the den
itself functioning as an open connection at the room level, wasn't actually covered).

**Final, corrected mapping, now implemented:**
- `RoomExit` → **locked** (ordinary room-to-room door — "room to room PIPES")
- `RegionTransportation` → **locked** — Rain World's long-distance pipe/tube system, the
  "internal-looking pipe that actually leaves the room" the user originally pointed at ("marked by
  triangles")
- `Normal` → **unlocked** (purely in-room shortcut, never leaves the room — "within room
  SHORTCUTS")
- `NPCTransportation` → **unlocked** (within-room creature-transport shortcut, despite the name —
  "within room TRANSPORT for creatures")
- `CreatureHole` → **locked** (den entrance — "creature DENS")

**Fix, in `ChainsGimmick.cs`:** `IsSealableExit(ShortcutData.Type)` now returns true for
`RoomExit`/`RegionTransportation`/`CreatureHole` (flipped from the first attempt's
`RoomExit`/`NPCTransportation`/`RegionTransportation`) and gates `LockNow`'s seal loop. The same
latent `FreePlayer` bug identified in the first pass still applies and is still fixed the same way:
`ChainState` remembers each sealed index's own original `Type`
(`ChainedShortcutOriginalTypes`, a `Dictionary<int, ShortcutData.Type>` alongside the existing
`ChainedShortcuts` index list) and `FreePlayer` restores each one to its own remembered type instead
of a hardcoded constant — so a sealed region pipe or den hole goes back to being exactly what it
was, not a generic door, regardless of which of the three sealable types it originally was.

**2026-09-13, same day, second bug: "normal shortcuts" stayed locked too.** The user rebuilt and
reported the corrected mapping still didn't work: within-room (`Normal`) shortcuts remained unusable
while a room was locked. Root cause was in `UpdateLock`'s belt-and-suspenders code, not
`IsSealableExit` (which was already correct) — two spots unconditionally cancelled ANY in-progress
shortcut entry every tick, regardless of which shortcut it was:

- the player's own `self.enteringShortCut.HasValue` check (just shoved the player back out of
  *whatever* shortcut they were entering, sealed or not)
- the identical per-tick check for every tracked creature's `creature.realizedCreature
  .enteringShortCut`

This is ported byte-for-byte from the source mod's own `CustomHRGuardManager.Update`, and in the
source mod it was harmless: since the source only ever seals `RoomExit`, "entering *a* shortcut"
and "entering *a sealed* shortcut" were the same condition there. That stopped being true the moment
this port started leaving `Normal`/`NPCTransportation` open on purpose (the fix above) — the
cancellation code didn't get the memo, so it kept firing for every shortcut in the room, sealed or
not, silently overriding `IsSealableExit`'s correct room-level type flip. This is exactly why the
user saw "normal shortcuts" still locked even though the underlying `ShortcutData.Type` was correct.

**Fix:** both cancellation checks now also require `state.ChainedShortcuts.Contains(...)` on the
shortcut index in question — i.e. only cancel entry into a shortcut this gimmick actually sealed.
`creature.creatureTemplate.doesNotUseDens = true` (the den-retreat AI flag, a separate mechanism from
shortcut-entry cancellation) is left unconditional, since it only affects AI den-seeking behavior,
not shortcut usability, and dens are always one of the sealed types anyway. Pushed
`ChainsGimmick.cs` (70,330 bytes, verified byte-exact on the first attempt).

**2026-09-13, third bug this same day: wrong type assumed for `Creature.enteringShortCut`, build
failure.** The user ran `dotnet build` against the fix above and got CS1503 on both new guard lines:
`cannot convert from 'RWCustom.IntVector2' to 'int'`. The guard code had assumed
`self.enteringShortCut`/`creature.realizedCreature.enteringShortCut` was a `Nullable<int>` shortcut
*index* (reasonable-looking given `state.ChainedShortcuts` is a `List<int>` of indices, but never
actually verified against the real field) and called `state.ChainedShortcuts.Contains(...)` on it
directly. Rather than guess again, dumped `Creature`'s own field table straight from
`lib/PUBLIC-Assembly-CSharp.dll`'s metadata (Field/Signature blobs, via `scratchpad/dnparse
/dnmeta.py` + `sigdecode.py`, decoding the raw field signature bytes) to get the real, compiler-true
type: `enteringShortCut` is `Nullable<RWCustom.IntVector2>` -- the *tile* the body is entering
through, not an index into `room.shortcuts` at all. Cross-checked `ShortcutData.StartTile` the same
way (dumped its `get_StartTile` method's return type from the same metadata): also `IntVector2`, and
already known to be the right "entrance tile" half of a `ShortcutData` from this same file's
`MultiChain` chain-anchor code (which already calls `room.shortcuts[shortcutIndex].StartTile` for
these same indices). Confirmed `IntVector2` has both `Equals` and `op_Equality` (`==`), so a direct
tile comparison is safe and idiomatic. Fix: new `IsSealedShortcutTile(Room, ChainState, IntVector2)`
helper that walks `state.ChainedShortcuts` and returns true if any sealed index's `StartTile` equals
the given tile; both `UpdateLock` guard sites now call this instead of the type-mismatched `.Contains`
call. Pushed `ChainsGimmick.cs` (71,638 bytes, verified byte-exact on the first attempt).

**Confirmed in-game by the user: "now it worked."** Third time was the charm for this specific
compile/logic chain -- the CS1503 build error is what actually surfaced the enteringShortCut type
mistake, so the fix that shipped was already correct on the first try once it compiled. Room-to-room
pipes and creature dens seal, within-room shortcuts and creature-transport stay usable while locked.

**2026-09-13, full re-audit of the source mod against this port, at the user's request** (after the
Chains fixes above were confirmed working, the user asked to double-check the whole
`artificer_rubicon_hell_bonanza.dll` for anything else not yet ported). Rather than re-read only the
methods already known to matter, enumerated every type in the DLL fresh (`Artificer_Hell`,
`Artificer_HellOptions`, `MultiChain`, `CustomHRGuardManager` -- confirmed via metadata this really
is the whole mod, only 4 types) and disassembled every method on all four that hadn't already been
looked at line-by-line, cross-checking every field/return type actually used against the real game
assembly's metadata rather than assumed. Four real findings, one of them a serious existing bug (not
just a missing feature):

1. **"Faux-hunted burden" (`ChainsHuntedModeHook`) had its room comparison backwards -- the
   feature was barely doing anything.** Disassembling `ArtificialIntelligence_Update` in full
   (previously only partially looked at) shows the source's actual logic: walk the player list;
   the instant ANY player already shares the creature's room, `return` immediately and do nothing
   (`beq` branching straight past the VultureAI check and the `SeeCreature` call to the method's own
   `ret`) -- ordinary in-room AI/vision already covers a colocated player, nothing to add there.
   Otherwise, force the first still-valid, *different-room* player onto that creature's `Tracker`
   (`Tracker.SeeCreature`, bypassing stealth/distance/line-of-sight entirely) and stop. That's the
   whole point of "hunted mode": creatures elsewhere on the map secretly hunt the player across room
   boundaries. This port's existing `ChainsHuntedModeHook` had `player.room != room` as its *skip*
   condition -- the exact inverse -- so it only ever acted on a player already sharing the room
   (a near no-op; ordinary vision mostly already covers that) and never touched a farther-away
   player at all, which is the one thing the feature is supposed to do. Fixed to match the source's
   actual control flow: bail out entirely (`return`) the moment any player shares the room, act on
   the first still-qualifying different-room player, then stop (the source doesn't loop over every
   match either -- `SeeCreature` fires once per tick per creature, then `ret`s). The per-player
   Primal/GimmickAssignments/FauxHuntedBurden gate this port already had is preserved, deliberately
   checked only at the "act on this player" step rather than the "bail out" step, since the source's
   own bail-out is unconditional on any colocated player regardless of that player's own settings
   (the source has no per-character concept at all -- single global toggle).
2. **`GetCreatureList`'s initial room scan never excludes a friendly/tamed lizard.** The friend
   check (`lizard.AI.friendTracker?.friend == self`) was already correctly ported into
   `TryAddArrivingCreature` (matching the source's own `AddNewCreature`, confirmed identical via
   disassembly), but the source's *other* creature-collection method, `GetCreatureList` -- the
   initial full-room scan run when a room is first entered -- never has this check at all, in the
   source's own compiled code. That's a real gap in the source itself: a lizard that's already your
   friend before you even walk in gets swept into the lock same as a hostile creature. The source
   papers over this belatedly via `DoRedundancyCreatureCheck`, a periodic (every 400 ticks, ~10
   seconds) sanity sweep that removes wrong-room/dead/InDen/banned/friendly-lizard creatures and, if
   the creature-count vs. chain-count still disagrees after that, nukes and fully rebuilds both
   lists from scratch -- logging lines like "It's still shit!!! Nuclear option!!" along the way.
   That whole mechanism is fundamentally the source author's own damage control for bugs this port
   doesn't have (this port's `GetCreatureList` already clears its list before rescanning, and every
   creature-list mutation elsewhere is already `Contains`-guarded -- see the 2026-09-12 arrival-hook
   entry above), so porting the belated 10-second self-heal wholesale isn't worth it just to close
   this one real gap. Instead, applied the same friend-check `TryAddArrivingCreature` already has
   directly inside `GetCreatureList` too, so a friendly lizard already in the room is never added at
   all, with no ~10-second window where it could be.
3. **`IsArenaSession` guard, never ported.** `CustomHRGuardManager.Update`'s very first real check
   (after an always-false, provably dead `wantsToDispose` field -- confirmed never assigned anywhere
   in the source's own compiled IL, across every method that touches the class) is
   `if (room.game.IsArenaSession) return;` -- the source disables the entire gimmick outside a story
   run. This port's own gate is `PrimalSlot.IsActive` (`rainWorld.options.saveSlot ==
   ReservedSlot`), and that's a *persistent* option -- switching from a story campaign straight into
   Arena/Sandbox without restarting the game does not reset it -- so without this check, a room
   entered in Arena mode while the last-used story slot happened to be Primal's reserved one could
   still trigger a lock. Ported as a hard early-out in the main `Player.Update` postfix, matching the
   source's own placement (checked before any room-change handling at all).
4. **`GW_A24` (Garbage Wastes) hardcoded room exclusion, never ported.** `Player_NewRoom`'s very
   first check, before it even records the new room, is `if (room.abstractRoom.name == "GW_A24")
   return;` -- left completely unexplained in the source's own code, exactly like the VultureAI skip
   elsewhere. The source's version freezes its single global manager on whatever it was previously
   tracking rather than touching this room at all; this port's per-player `ChainState` model has no
   equivalent "previous manager" to freeze, so the closest faithful equivalent -- simply never
   starting a lock in this one specific room -- was added as a plain hardcoded room-name check in
   the same room-change block as the Arena guard above, kept unexplained exactly as the source
   leaves it (not guessed at).

Also explicitly re-confirmed, not changed: `VultureAI.CreatureSpotted` remains a known, documented,
deliberate gap (this file's header comment already covers it -- vultures that merely spot the player
from outside the room don't join a lock in this port, same as before). The rest of
`DoRedundancyCreatureCheck` (wrong-room/dead/banned/count-desync repair) is deliberately not ported
for the reason given in finding 2 above -- this port's own list-management doesn't develop the
desync that machinery exists to repair. `Artificer_HellOptions.Initialize`'s banlist-preset
construction, `RainWorldGame_ctor`/`RainWorld_OnModsInit` (options registration, matches this port's
own `RainWorldHooks.cs`), and `Creature_NewRoom`/`Player_NewRoom`'s remaining buffered-arrival-flush
logic were all re-checked against this port's existing `TryAddArrivingCreature`/`FlushArrivalBuffer`
and found to already match.

Pushed `ChainsGimmick.cs` with all four fixes (77,586 bytes, verified byte-exact on the first
attempt).

**2026-09-13, same day: user reviewed the four findings and rejected one.** "remove hardcode on
GW_A24 / allow tamed lizards to not be chained." The tamed-lizard fix (finding 2 above) was already
correct as shipped -- no change needed there, it stays exactly as implemented (both
`TryAddArrivingCreature` and `GetCreatureList` skip a `Lizard` whose `friendTracker.friend == self`).
The `GW_A24` hardcode (finding 4 above), though faithfully ported from the source, is something the
user explicitly does not want in this mod: an unexplained, hardcoded room-name special case. Removed
outright -- `TryLockRoom` is now called unconditionally on every room change again, with only a
comment left behind explaining what was here and why it was taken back out (so a future session
doesn't reintroduce it while re-auditing the source again). The `IsArenaSession` guard (finding 3)
was not objected to and stays. Pushed `ChainsGimmick.cs` again (77,026 bytes, verified byte-exact on
the first attempt).

**Not yet rebuilt or tested in-game** -- next: `dotnet build`, redeploy, and specifically check (1)
with "Faux-hunted burden" on, a creature in a different, unlocked room actually starts
pathing toward/hunting the player (previously it did nothing meaningful); (2) a lizard that's
already befriended before you enter a room is not added to the lock (no lock triggers at all if it's
the only creature present); (3) Arena/Sandbox mode never triggers Chains even if the last story
save slot used was Primal's reserved one; (4) Garbage Wastes room A24 now behaves like any other
room (locks normally if it has qualifying creatures, no special-case exemption).

**Update (2026-09-15, later session -- direct user reports from real play, all three fixed): (1)
den chain-visuals removed, (2) creatures could escape via unguarded shortcuts, (3) LC_SUBWAY02
unlocked early.** Three reports in one message, and the second and third turned out to share one
root cause.

1. **"the graphics for chains should not come from creature only pipes - it should come only from
   pipes slugcat can enter to travel between rooms."** `MultiChain`'s constructor was handed
   `state.ChainedShortcuts` -- every sealed exit, including `CreatureHole` dens -- and drew one
   `VoidChain` visual per entry, anchoring one at a den hole the player could never actually walk
   through in the first place. Fixed by adding a second, narrower list, `ChainState.PipeShortcuts`
   (built alongside `ChainedShortcuts` in `LockNow`, filtered through a new `IsPlayerPipe` helper --
   true only for `RoomExit`/`RegionTransportation`) and handing THAT to `MultiChain` instead
   (`AssignNewChain`). The den itself is still sealed and still defended exactly as before -- only
   which sealed exits get a chain drawn on them changed.
2. **"creatures seemed to be able to go through their creature only pipes, either dens or in room
   shortcuts for them - they shouldnt be able to do that. they can only go through pipes and
   shortcuts, the same ones slugcat can go through."** Root cause: the per-tick creature-side
   `enteringShortCut` cancellation (`UpdateLock`) only ever cancelled entry into a tile that was in
   `state.ChainedShortcuts` -- i.e. one of the three ROOM-sealed types. `NPCTransportation` (the
   within-room creature-transport shortcut) was deliberately never added there, on the theory it
   "never leaves the room" the same way `Normal` doesn't -- true of the room-to-room question, but
   the user's own correction reframes the actual rule: creatures should be held to exactly what the
   player could use, and the player never has access to `NPCTransportation` (or `CreatureHole`) at
   all. So a tracked creature riding an `NPCTransportation` shortcut was never guarded against by
   anything, room-locked or not. Fixed by replacing the old tile-membership check
   (`IsSealedShortcutTile`, deleted) with `TryGetShortcutTypeAtTile` (resolves a tile to its real
   type, sealed or not, via `ChainedShortcutOriginalTypes` falling back to the live `room.shortcuts`
   type) plus a new `IsRestrictedForCreature` predicate -- `IsSealableExit`'s three types PLUS
   `NPCTransportation` -- so creatures are now blocked from all four non-`Normal` types regardless
   of which ones happen to be sealed at the room level. The player's own equivalent check still
   uses `IsSealableExit` alone (correct as-is -- the player never legitimately touches the other
   two types anyway). Also added a second, independent hardening: the one-shot `ConfuseCreatures`
   redirect (only ever ran once, at the exact tick a room locked) is now also called every tick from
   `UpdateLock` for every still-tracked creature (`ConfuseCreature`, extracted out so both call sites
   share it) -- a creature that decided to flee toward its den a moment AFTER the initial lock, not
   at the moment of it, was never redirected by the old one-shot call.
3. **"in LC_SUBWAY02, the room opened prematurely, before all creatures were killed."** Direct
   consequence of finding 2, not a separate bug: a creature that slipped through an unguarded
   `NPCTransportation`/`CreatureHole` shortcut and reached `InDen` was force-killed by `UpdateLock`'s
   existing `if (creature.InDen)` branch (there specifically so a creature can't just vanish into a
   den this gimmick can no longer see) -- which silently emptied `state.CreatureList` and unlocked
   the room the instant that ONE creature was gone, even while others were still alive. Fixing
   finding 2 removes the escape route this depended on; no separate change was needed for this
   report specifically.

Pushed `ChainsGimmick.cs` with all of the above -- `IsSealableExit`/`IsPlayerPipe`/
`IsRestrictedForCreature` all live together near the top of the file now, `ConfuseCreature` is a
new extracted method, `TryGetShortcutTypeAtTile` replaces the deleted `IsSealedShortcutTile`, and
the file's own 2026-09-13 header note was updated in place rather than left describing the now-wrong
"NPCTransportation stays open for creatures" rule. Built clean, deployed. **Not yet re-confirmed
in-game** -- next, in a room with more than one creature: (1) chain visuals only ever appear
anchored at real doors/pipes, never at a den hole; (2) a creature can no longer be seen using a
den or an in-room creature-transport shortcut while the room is locked -- confirm by watching a
lizard or similar try to flee and getting redirected/blocked instead of vanishing; (3) specifically
in LC_SUBWAY02 (or any multi-creature room), the room stays locked until every tracked creature is
actually dead, not just until one of them reaches a den; (4) the escape timer and manual
kill-everything unlock paths still both work normally, since neither was touched.

## Primal Slot Tab-switch, in detail (base mechanic + round-3 transition confirmed working in-game; round-4 character-preservation and round-5 multiple-Primal-slots both pushed together, unbuilt/untested)

Requested (2026-09-13): "in the character selection menu, re-add Tab to switch save slots, to
primal character slot and back. use the same code that runs the normal changing save slot. If it
takes some time, you can add a black screen while it is loading -- we will eventually want some
sort of a transition, so, we can add some of that logic already if needed." This is a re-add: an
earlier attempt at a Tab-switch idea was discarded (see History below) -- this time it's scoped
narrowly to just the slot switch, and the user explicitly asked to reuse the same machinery the
PRIMAL SLOT Options-menu button already drives, rather than anything new.

**What it does.** Pressing Tab while `Menu.SlugcatSelectMenu` is the active menu toggles
`options.saveSlot` between whatever real slot was active and `PrimalSlot.ReservedSlot`, exactly
the way clicking the PRIMAL SLOT Options button already does, then rebuilds the character-select
screen against the new slot. A full-screen black `FSprite` covers the screen for as long as
`PlayerProgression` is still mounting (usually instant, occasionally not -- hence "if it takes some
time"), and remains up until the rebuilt `SlugcatSelectMenu` actually exists again.

**Why this can't just call `SetCurrentlySelectedOfSeries` directly.** That's an instance method on
`Menu.OptionsMenu`, and its whole busy/poll/revert cycle lives in `OptionsMenu`'s own private
fields (`waitingOnProgressionLoaded`, `leavingSaveSlot`) and its own `Update()` loop -- there's no
live `OptionsMenu` instance to call it on from `SlugcatSelectMenu`. Disassembling that method (this
session) confirmed its `"SaveSlot"` branch is exactly three operations:

```
options.saveSlot = to;
rainWorld.progression.Destroy(leavingSaveSlot);
rainWorld.progression = new PlayerProgression(rainWorld, true, false);
```

`PrimalSlotTabSwitchHook.cs`'s new `PrimalSlotSwitcher` class replicates these three operations
directly, with its own polling (see below) standing in for `OptionsMenu.Update`'s. Same
operations, same order, same success/failure result codes -- just not routed through a live
`OptionsMenu`, because there isn't one on this screen. This is "the same code that runs the normal
changing save slot" in the sense the user meant it (the actual mechanism, not a literal method
call that isn't reachable from here).

**Polling, not a callback.** `OptionsMenu.Update()` (disassembled this session) polls
`rainWorld.progression.progressionLoaded` every frame until true, then checks
`progressionLoadedResult`: 1 (`SUCCESS_LOAD_EXISTING_FILE`), 2 (`SUCCESS_CREATE_NEW_FILE`), and 5
(`ERROR_SAVE_DATA_MISSING`, treated as a normal "new/empty slot" case) all mean success; anything
else is a failure that `HandleSaveSlotChangeFailed` reverts. Since the character-select menu is
about to be torn down and rebuilt mid-switch (it can't poll its own demise), `PrimalSlotSwitcher`'s
poll is instead driven by a new Postfix on `RainWorld.Update` -- runs every frame regardless of
which menu, if any, is currently active. On success it calls
`ProcessManager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlugcatSelect)` to rebuild the
menu; on failure it reverts (same three operations, back to the original slot) and re-polls that
revert's own load the same way, mirroring `HandleSaveSlotChangeFailed`'s behavior minus the
in-game dialog (see "Deliberately simplified" below).

**The black screen, and the transition "for free."** `ProcessManager.InitFadeSprite` (disassembled
this session, to get this right rather than guess) builds vanilla's own process-switch fade cover
as `new FSprite("Futile_White", true)`, centered at `rainWorld.screenSize / 2`, no explicit
`scaleX`/`scaleY` -- confirming that specific atlas element + constructor overload is what reliably
covers the whole screen regardless of resolution. `PrimalSlotSwitcher.ShowBlackScreen` uses the
exact same construction, but held fully opaque (`alpha = 1`) for the whole waiting period, rather
than vanilla's animated fade-in -- a hard "black screen while loading," per what was asked for.
Calling `RequestMainProcessSwitch` on success is what gives this a real transition "for free": that
call triggers vanilla's own black-fade-and-back around the rebuild, so our own black screen only
has to survive until the rebuilt `SlugcatSelectMenu` exists (a new Harmony Postfix on that class's
own constructor, `PrimalSlotTabSwitchMenuRebuiltHook`, is what drops it at exactly that moment,
before the new instance is ever drawn) -- this is the "first step toward a future fuller transition
system" the request asked for, achieved by reusing vanilla's existing fade rather than building a
new one.

**Deliberately simplified vs. `HandleSaveSlotChangeFailed`:** vanilla shows a translated
`Menu.DialogNotify` failure dialog on a failed switch; this port just logs the failure via
`Plugin.Log` and reverts silently, to avoid adding `Menu.DialogNotify`'s constructor,
`ProcessManager.ShowDialog`, and `Menu.DialogBoxNotify.CalculateDialogBoxSize` to the pile of
genuinely-new, never-before-touched API surface this feature already needs (see GOTCHA #1 note
below) for what should be a rare edge case. If a failed switch turns out to need to be visible to
the player in practice, revisit this.

**GOTCHA #1, applied here specifically.** `PlayerProgression`'s constructor/`Destroy`/
`progressionLoaded`/`progressionLoadedResult` and `ProcessManager.RequestMainProcessSwitch`/
`IsRunningAnyDialog` are all genuinely new surface -- nothing in this codebase has touched any of
them before this feature. Every one of them is resolved via `HarmonyLib.AccessTools`
(`AccessTools.Field`/`Method`/`Constructor`/`Property`) and invoked through reflection rather than
plain C# member access, matching the same defensive pattern already used for `PlayerGraphics.player`,
`SlugcatStats`'s stat fields, and `ConfigHolder.Save()` elsewhere in this codebase -- specifically
so that if any of these turn out to be genuinely non-public at runtime (the publicized reference
DLL's accessibility flags can't be trusted, confirmed multiple times already in this project), the
failure is a caught, logged exception rather than a `MethodAccessException`/`FieldAccessException`
crash. `rainWorld.options.saveSlot` is the one exception, read and written directly -- `PrimalSlot.cs`
already does this everywhere in shipped, working code, so it's treated as proven. **This is an
open risk that can only be resolved by an actual build + in-game test, not more disassembly** --
watch the BepInEx log the first time Tab is pressed; a null `AccessTools` lookup logs clearly
rather than crashing, but means that particular operation silently no-ops instead of switching.

**Also confirmed via metadata during this feature's research (not previously touched in this
project):** `Menu.SlugcatSelectMenu.Update()` is declared directly on that class (not inherited
unoverridden from `Menu`), so patching it by name only ever fires for this specific menu; its
constructor takes exactly `(ProcessManager)`, matching `Menu.OptionsMenu`'s own ctor shape reused
by `PrimalSlotButtonConstructionHook`; and `ProcessManager.ProcessID.SlugcatSelect` (not
`SlugcatSelectMenu`, which is only the class name) is the correct enum member for this screen --
caught before writing any code, not after a failed build.

**Update (2026-09-13): confirmed working in-game.** The user built and tested this and reported
"surprisingly, it works, well done" -- Tab switches into and back out of Primal's reserved slot,
using the base mechanic exactly as designed above. No fixes were needed; this was a first-try
success, unlike most other features in this doc.

**Update (2026-09-13): vertical slide transition, requested as a follow-up.** The user asked for
an actual camera-style transition on top of the (confirmed-working) instant switch: "move the
camera down (or move the ui up) and then move down into the primal versions. When switching back,
reverse." Read as one continuous vertical pan per switch direction (not two independently-aimed
phases): entering Primal, the whole screen slides up and off the top, then the rebuilt screen
slides up into place from below -- everything moves the same direction throughout, like a camera
continuously descending past the normal-characters screen to arrive at the Primal one sitting
"below" it. Leaving Primal is the exact mirror: slides down and off the bottom, then the restored
screen slides down into place from above.

Researched before writing anything (this codebase's usual discipline, not skipped just because the
base feature already shipped): disassembled `Menu.PositionedMenuObject` (which `Menu.Page` extends
unoverridden) and confirmed `Update()` is `base.Update(); lastPos = pos;` every tick, and
`get_ScreenPos()` is `owner?.ScreenPos + pos` computed recursively up the owner chain. Two things
fall out of that: moving a `Page`'s own `pos` moves everything anchored under it as one unit (the
same reason `self.pages[0]` already works as "the one page that holds everything" in
`PrimalSlotOptionsMenuHooks.cs`), and RW's own `pos`/`lastPos`-with-timeStacker interpolation is
already the standard smooth-motion mechanism throughout vanilla's UI -- so animating `pos.y` by a
fixed step once per simulation tick (from a Postfix on `SlugcatSelectMenu.Update`, which runs
*after* the real `Update()` has already done its own `lastPos = pos` snapshot for this tick) gets
smooth motion for free, no custom interpolation code needed. Also confirmed `Menu.Menu.pages` is
the exact field name/type already relied on elsewhere in this codebase.

Implementation (`PrimalSlotTabSwitchHook.cs`, extended, not rewritten): a `TransitionPhase` enum
(`None`/`SlideOut`/`SlideIn`) added to `PrimalSlotSwitcher.State`. Tab press now starts `SlideOut`
instead of calling `BeginSwitch` directly; `StepSlideOut` (driven by the same
`SlugcatSelectMenu.Update` Postfix that used to just detect Tab) moves the *current, still fully
rendered* menu's `pages[0].pos.y` toward `+screenSize.y` (entering) or `-screenSize.y` (leaving)
over `SlideDurationTicks` (10 ticks, ~0.25s) ticks; once it reaches the edge, only then does
`BeginSwitch` run -- the actual save-mount and existing black-screen cover are completely
unchanged and still only cover the window where the screen has nothing readable on it anyway.
Once the menu rebuilds, `OnSlugcatSelectMenuRebuilt` (the existing ctor Postfix) now also calls a
new `BeginSlideIn`: positions the new page off the matching edge (both `pos` *and* `lastPos`, to
avoid a one-frame Lerp-from-the-construction-default-position glitch) and starts `SlideIn`, which
`StepSlideIn` animates back to `pos.y == 0` the same way. A staleness guard was added to the
`Update` Postfix (compares `state.SlidingPage` against the live instance's own `pages[0]`) so that
if the player backs out of the menu entirely mid-slide, the stale transition state is dropped
instead of leaving Tab permanently unresponsive on whatever menu they land on next.

Deliberately did **not** relayer the black-screen `FSprite` behind the sliding content (which would
have needed `FNode.MoveToBack()` and an assumption about Futile.stage's child draw order that
can't be confirmed without a live test) -- instead the slide phases only ever run while a real,
fully-rendered menu is what's on screen (the still-live old one before the mount, the freshly-built
new one after), so the black cover's existing behavior (shown only across the actual mount/load
window, exactly as already shipped) needed no changes at all. This was a deliberate scope-narrowing
to avoid adding a second, riskier kind of new ground (rendering z-order) on top of the first
(reflection-guarded `PlayerProgression`/`ProcessManager` surface, still the same open risk
documented above and unchanged by this addition).

Pushed `PrimalSlotTabSwitchHook.cs` (32,881 bytes, verified byte-exact on the first
`device_commit_files` attempt). **Not yet built or tested** -- next: `dotnet build`, then in-game:
(1) entering Primal's slot visibly slides the screen up and off the top, pauses (mount), then the
new screen slides up into place from below, with no visible jump/pop at either handoff; (2)
leaving Primal is the exact mirror (down and off the bottom, then down into place from above); (3)
backing out of the character-select screen mid-slide (if that's even reachable given the ~0.25s
window) doesn't leave Tab broken afterward; (4) still check `BepInEx/LogOutput.log` for any
`PrimalSlotSwitcher` line, same as before -- this addition doesn't touch the reflection-guarded
code paths but shares the same log-and-continue discipline for its own new failure paths
(`BeginSlideOut`/`BeginSlideIn` logging when `pages[0]` is unexpectedly missing).

**Update (2026-09-13): round-1 slide confirmed working, with two bugs reported.** The user built
and tested the slide transition and reported: "alright, well, the buttons moved, nice, but the
background didnt -- gotta move the background as well, like, the whole thing. also, dont do a
linear transition but a smooth one, ease in ease out." Two distinct fixes, addressed together as
round 2 since both touch the same stepping code.

**Fix 1: the background wasn't moving.** Round 1 only ever moved `pages[0].pos.y` -- the
character-select screen's foreground buttons/UI live under that page, but the background artwork
lives on `Menu.Menu.scene` (a `Menu.MenuScene`), which is a sibling of `pages`, not a child of
`pages[0]`. Moving the page alone was always only ever going to move what's parented under it.

Before writing anything, disassembled `Menu.MenuScene` and its illustration types fresh (this
codebase's usual discipline) rather than assuming the obvious-looking `camPos` field ("camera
position", matching the user's own wording of "move the camera down") was the right lever.
`MenuScene.GrafUpdate` disassembled to `Shader.SetGlobalVector(ShadPropMenuCamPos, ...)` fed from
`CamPos(t) = Lerp(lastCamPos, camPos, t) * cameraRange` -- confirming `camPos` only ever drives a
small shader-level global uniform for a subtle mouse-parallax wobble, normalized into a small range
by `cameraRange`, not an actual object-space repositioning. Pushing that a full screen height would
almost certainly either do nothing visible (clamped/normalized away) or break in some
shader-specific way it's not possible to predict from disassembly alone -- so `camPos` was
deliberately **not** reused, despite matching the user's literal wording.

Instead: `Menu.MenuScene` extends `Menu.MenuObject` directly (not `PositionedMenuObject` -- it has
no `.pos` of its own), but it holds the actual pieces of art as
`depthIllustrations`/`flatIllustrations` (`List<Menu.MenuDepthIllustration>`/`List<Menu.MenuIllustration>`)
plus two named singles, `scribbleA`/`scribbleB` (`Menu.MenuIllustration`). Both illustration types
extend `Menu.RectangularMenuObject` which extends `Menu.PositionedMenuObject` -- so every individual
piece of background art has its own inherited `.pos`/`.lastPos`, exactly like a `Page` does, and
since each one's `owner` is `scene` (not itself a `PositionedMenuObject`), its `ScreenPos` is just
its own `.pos` with no recursive offset to account for. Also disassembled `MenuScene.Update()` to
check nothing else writes these `.pos` fields every frame and would fight the slide: the only writer
found is a dev-tools mouse-drag debug path gated behind `devToolsActive`, irrelevant in normal
play -- confirming illustrations' `.pos` is set once at scene-build time and is safe to nudge
directly.

New `CollectSlidingObjects(Menu.SlugcatSelectMenu menu)` helper gathers `menu.pages` (all of them,
not just `pages[0]`, null-checked) plus `menu.scene.depthIllustrations`, `flatIllustrations`,
`scribbleA`, and `scribbleB` (all null-checked) into one flat
`List<Menu.PositionedMenuObject>`. Both `BeginSlideOut` and `BeginSlideIn` now call this instead of
touching `pages[0]` alone, and the stepping code (`StepSlide`, see Fix 2) moves every object in the
list by the same delta each tick -- so the whole screen, buttons and background alike, moves as one
unit, matching "move the whole thing" literally. `State.SlidingPage` (a single `Page`) was replaced
with `State.SlidingObjects` (a `List<Menu.PositionedMenuObject>`) throughout.

Open risk, not resolvable by disassembly and not previously flagged: whether the background
illustrations are actually tall/large enough that shifting them a full screen height doesn't reveal
a gap or seam at the edge they slide away from. Vanilla was never designed to move this art at all,
so there's no guarantee it extends far enough past the visible frame. This is a genuine
in-game-only question, distinct from the GOTCHA #1 reflection risk below.

**Fix 2: linear motion, asked to be eased in/out.** Round 1 stepped `pos.y` by a fixed amount every
tick (`screenSize.y / SlideDurationTicks`) -- constant velocity, felt mechanical rather than smooth.

New `EaseInOutFraction(int ticksElapsed, int totalTicks)` computes a sinusoidal ease:
`0.5 - 0.5 * cos(pi * clamp01(ticksElapsed / totalTicks))` -- a standard ease-in-out curve, zero
velocity at both the start and end of the slide, smoothly accelerating through the middle. Rather
than stepping every object by a fixed amount, the new shared `StepSlide` helper evaluates this
fraction *before* and *after* incrementing `state.SlideTicksElapsed` each tick, and moves every
object in `SlidingObjects` by the **difference** between those two fractions (times `screenSize.y`
and a `+1`/`-1` direction) -- i.e. an eased delta per tick, not an eased absolute position
recomputed from some fixed start point. This shape was chosen deliberately so the same stepping
code keeps working correctly no matter how many objects are in `SlidingObjects` or what each one's
own starting `.pos` happens to be, since it only ever needs to know "how far to nudge everything
this tick," not "where should object N be at time T." `BeginSlideOut`/`BeginSlideIn` and their
`StepSlideOut`/`StepSlideIn` wrappers were rewritten as thin callers into this one shared
`StepSlide`, taking a completion callback (`BeginSwitch` for the out-phase, nothing for the
in-phase) so the eased-stepping logic itself only exists once. `SlideDurationTicks` was bumped from
10 to 16 (~0.4s at 40 ticks/sec) to give the ease curve room to actually read as eased rather than
being over almost as soon as it starts.

Pushed `PrimalSlotTabSwitchHook.cs` (39,230 bytes, verified byte-exact on the first
`device_commit_files` attempt). **Not yet built or tested.** Next: `dotnet build`, then in-game
confirm (1) the background art visibly moves together with the buttons during both slide phases,
with no seam/gap revealed at either edge (the open risk flagged above); (2) the motion visibly
accelerates then decelerates rather than moving at constant speed; (3) both directions still slide
smoothly with no pop/flash at the mount handoff, same as round 1's checklist; (4) check
`BepInEx/LogOutput.log` for any `PrimalSlotSwitcher` line, same discipline as before -- this round
doesn't touch the reflection-guarded `PlayerProgression`/`ProcessManager` surface at all, but shares
the same log-and-continue pattern for its own new failure path (`CollectSlidingObjects` returning
empty logs a warning and skips the animation rather than crashing).

**Update (2026-09-13): round 2 confirmed NOT working; round 3 replaces it with a different,
simpler mechanism.** The user tested round 2 and reported: "more easing, and the background still
doesnt move. you can leave the issue of what it will look like while transitioning later, ill see
whats there and let you know when the background moves." So round 2's background fix genuinely did
nothing, and the easing needed to be stronger.

Rather than patch round 2's approach again, disassembled `Menu.SlugcatSelectMenu`'s own constructor
fresh, line by line, specifically looking for wherever it builds its `scene`. It never does.
`Menu.Menu`'s own base constructor (also disassembled, in full) doesn't touch `scene` either — it
only ever sets `pages`, `container`, `cursorContainer`, `currentPage`, and `infoLabel`. So
`menu.scene` is simply **null** for this particular menu, which is exactly why round 2's
`CollectSlidingObjects` (which null-checked `menu.scene` and moved on) always had zero background
illustrations to move — not a bug in the moving code, a wrong premise about where the background
lives. Whatever renders the visible background on this screen isn't a `MenuScene` illustration and
isn't necessarily a `PositionedMenuObject` at all, so chasing it object type by object type was
never going to reliably find it.

Instead of identifying the exact mechanism, round 3 sidesteps the question. Disassembled
`Menu.MenuObject.get_Container()` (a property every `MenuObject` inherits) and found it resolves
recursively: return `myContainer` if this object set its own; else `owner.Container` (recursive);
else, the base case for anything with no owner and no override — which includes every `Page`,
confirmed by checking `Menu.Page`'s own constructor and field list, neither of which ever sets
`myContainer` — return `menu.container`. Combined with confirming (via `Menu.Menu`'s own
constructor) that `container` is a plain `FContainer` created once and added straight to
`Futile.stage`, and confirming (by disassembling `Menu.Update`/`Menu.GrafUpdate` in full) that
nothing in the base `Menu` class ever touches it again afterward, this means `menu.container` is
the single Futile display-list node that **everything** this menu ever draws — every page, every
subobject, any scene, any raw sprite added directly — is parented under, directly or indirectly,
regardless of which internal mechanism was used to build it. Moving that one object moves the whole
screen, with no need to know how the background specifically is put together.

So round 3 replaces `CollectSlidingObjects`/`State.SlidingObjects` (a list of individual
`PositionedMenuObject`s) with a single `State.SlidingContainer` (an `FContainer`) and animates
`container.y` directly instead of a list of `.pos.y` values. `FContainer.y` is a plain public float
property inherited from `FNode` — the same base class `FSprite` (already used directly for the
black screen) extends — so no new reflection risk there. `Menu.container` (the field that gets us
the container in the first place) is, however, itself genuinely new surface never read directly in
this codebase before, so per this file's own GOTCHA #1 discipline it's fetched through a new
`AccessTools.Field(typeof(Menu.Menu), "container")` (`MenuContainerField`, plus a small
`GetMenuContainer` wrapper that logs and returns null on failure) rather than a plain `menu.container`
read, even though nothing about a `Menu`'s own container field looks unusual. One side benefit:
`FNode.y` has no separate interpolated draw position the way `PositionedMenuObject.ScreenPos`/
`DrawPos` does (that Lerps between `pos` and `lastPos` using the frame's timeStacker), so setting
`container.y` once at the start of the slide-in takes effect immediately — round 1/2's
`lastPos`-matching workaround for a one-frame Lerp-from-default-position glitch is no longer needed
at all.

For "more easing": swapped the sinusoidal ease (`0.5 - 0.5*cos(pi*t)`) for a cubic ease-in-out
(`4t³` for the first half of the slide, mirrored for the second) — noticeably sharper acceleration
and deceleration at each end than the sine curve gave — and raised `SlideDurationTicks` from 16
(~0.4s) to 30 (~0.75s), since the sharper curve needs more time on screen to actually read as eased
rather than mostly happening within a couple of frames. Still applied as the tick-to-tick
*difference* between the eased fraction at the previous and current tick, exactly as round 2's
`StepSlide` did — only the curve shape and duration changed, not the stepping mechanism itself.

The staleness guard in `PrimalSlotTabSwitchHook.Postfix` (dropping a transition if the player backs
out of the menu entirely mid-slide) now compares `state.SlidingContainer` against
`GetMenuContainer(__instance)` by reference instead of checking `pages[0]` membership in a list —
same protection, adapted to the new single-object model.

Per the user's own request, deliberately **not** re-litigating what the background will look like
mid-slide (seams, gaps, whether the art is tall enough to cover a full screen height of travel) —
that's now something to just look at once it's actually moving, not something to keep predicting
from disassembly.

Pushed `PrimalSlotTabSwitchHook.cs` (39,508 bytes, verified byte-exact on the first
`device_commit_files` attempt).

**Update (2026-09-13): confirmed working in-game.** The user built and tested round 3 and
reported "good, now it works" — the whole screen (background included) now slides together with a
visibly eased motion. No further fixes needed for the transition itself; see round 4 immediately
below for the next request (character preservation across the switch), raised in the same message.

**Update (2026-09-13): round 4, character preservation across the switch.** Immediately after
confirming round 3 worked, the user asked for a behavior fix, not a visual one: "when switching, it
doesnt switch between most recent in both save files but switches to the same character -- so if
you are on primal artificer, you switch to normal artificer. if you are on normal gourmand you
switch to primal gourmand." Without this, Tab-switching landed on whatever the OTHER slot's own
save data happened to default to, unrelated to whichever character was on screen.

Root cause, found by disassembling `Menu.SlugcatSelectMenu`'s constructor and `Update()` fresh: the
character-select screen shows exactly one character at a time (`slugcatPageIndex`, an int into
`slugcatPages`) and steps it by ±1 via a queued-input/scroll-swipe mechanism (`quedSideInput`,
`scroll`/`lastScroll`) driven by PREV/NEXT buttons -- there's no grid of clickable portraits. Every
place that changes `slugcatPageIndex` also calls the menu's own
`UpdateSelectedSlugcatInMiscProg()`, which disassembly showed does exactly
`progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat =
slugcatPages[slugcatPageIndex].slugcatNumber` — that persisted field is *written from* the
on-screen selection, never read back to restore it. The constructor's own initial-selection logic
(also disassembled) only auto-picks a character when that slot's save data has exactly one existing
character; otherwise it just leaves the index at its default (0). That default, not "most recently
played," is what the user was actually seeing.

The fix (`PrimalSlotTabSwitchHook.cs`, extended again): capture which character is showing on the
OLD menu right when Tab is pressed (`PrimalSlotSwitcher.GetSelectedSlugcat`, reading
`slugcatPages[slugcatPageIndex].slugcatNumber`), carry it through the existing per-switch `State` as
a new `TargetSlugcat` field, and once the rebuilt menu exists, find that same `SlugcatStats.Name` in
its own `slugcatPages` and point `slugcatPageIndex` straight at it (`ApplyCharacterFix`) — resetting
`scroll`/`lastScroll`/`quedSideInput` to a resting state (rather than queuing up N simulated
button-presses to visually walk there one step at a time) and calling
`UpdateSelectedSlugcatInMiscProg()` so the persisted field matches too, for any other system that
might read it later. Every field this touches (`slugcatPages`, `slugcatPageIndex`, `scroll`,
`lastScroll`, `quedSideInput`, `SlugcatPage.slugcatNumber`) is new surface never read directly in
this codebase before, so per GOTCHA #1 every one of them goes through a new `AccessTools.Field`/
`AccessTools.Method` lookup rather than plain member access — a failed lookup just leaves whichever
character the slot defaulted to (the pre-round-4 behavior), logged, rather than crashing.
`Menu.SlugcatSelectMenu.SlugcatPage` itself, used directly as a compile-time type (for the cast used
while scanning the page list), was trusted without reflection: its sibling nested class
`SlugcatPageNewGame` is already used exactly that way — including as a live Harmony postfix
parameter type — in this mod's already-shipped, already-working `PrimalCharacterNames.cs`, which
wouldn't function at all if that nesting weren't genuinely accessible.

One risk flagged and deliberately hedged against rather than chased down: each `SlugcatPage`'s own
on-screen position is computed incrementally frame-to-frame by its own `Update()`/`GrafUpdate()`
(not reverse-engineered in full), not recomputed instantly from `slugcatPageIndex`. Jumping the
index directly, instead of animating through it one step at a time the way a real button-press
would, risks a stale-looking frame or two right after the jump. Rather than reverse-engineer that
per-page animation fully, `OnSlugcatSelectMenuRebuilt` now defers dropping the black screen: it
applies the character fix, then leaves the new menu in a `PendingRevealMenu` state for a fixed
`RevealSettleFrames` (6) real `RainWorld.Update` ticks — handled by the existing `Poll`, which
already runs every frame — before finally calling `HideBlackScreen`/`BeginSlideIn`. That's a sixth
of a second of extra time already spent behind the already-opaque black screen, cheap insurance
against a glitch that (if the concern is even real) nobody will ever actually see.

Pushed `PrimalSlotTabSwitchHook.cs` (52,753 bytes, verified byte-exact on the first
`device_commit_files` attempt) as round 4. **Immediately followed by round 5 (below) before this
build was ever tested** — the two are documented separately here but shipped as one combined push.

**Round 5, multiple Primal slots (2026-09-13, same day, next request, before round 4 was
confirmed):** "instead of switching between save slot 1/2/3 and primal slot (666666) lets switch
between slot 1 and primal slot 1 (666666) then when in slot 2 you switch between slot 2 and primal
slot 2 (666667) and so for slot 3." Previously every real slot's Tab press led to the exact same
single reserved slot (`PrimalSlot.ReservedSlot`, 666666) — so Primal saves for real slots 1, 2, and
3 all lived in that one slot, overwriting each other, and Tab-ing back out relied on a single
process-global `PrimalSlot.LastRealSlot` that only remembered the most recent real slot, wrong the
moment Primal had been used from more than one real slot in the same session.

Fixed at the source, `PrimalSlot.cs` (see that file's own header comment, MULTIPLE PRIMAL SLOTS),
rather than by adding more state here: each real slot N now gets its own reserved slot,
`ReservedSlot + N`, via two new pure functions, `PrimalSlotFor(realSlot)` and its inverse
`RealSlotFor(primalSlot)` — a stateless mapping in both directions, so there's no separate variable
to keep in sync and nothing that can drift out of date. `IsActive` changed from an exact-equality
check (`saveSlot == ReservedSlot`) to a range check (`saveSlot >= ReservedSlot`), on the same
"nothing else plausibly generates numbers in this range" reasoning the original constant relied on
— now covering a small range instead of one exact value, still far outside anything Rain World's
own UI, a platform's save picker, or another mod would ever produce.

`PrimalSlotTabSwitchHook.cs`'s own change is small: `Postfix` now computes `targetSlot` from
whichever slot is *currently* mounted, via that same mapping in both directions
(`PrimalSlot.RealSlotFor`/`PrimalSlotFor`), instead of reading/writing `LastRealSlot` (removed
entirely from `PrimalSlot.cs` — nothing else referenced it). Everything downstream of `targetSlot`
— `BeginSlideOut`, `BeginSwitch`, the whole `PlayerProgression` mount/poll machinery, character
preservation from round 4 — needed no changes at all, since none of it ever hard-coded
`ReservedSlot` itself; it all already just treats `targetSlot` as an opaque slot number.

The Options-menu "PRIMAL SLOT" button (`PrimalSlotOptionsMenuHooks.cs`) needed the equivalent fix
for consistency, since it shares the same underlying `PrimalSlot.IsActive`/reserved-slot concept:
without it, clicking that button from real slot 2 or 3 would have landed on real slot 1's Primal
save (666666) instead of its own, while Tab from that same screen would correctly go to 666668 —
two different, disagreeing ideas of "the" Primal slot for the same real slot. Fixed the same way:
`PrimalSlotGetSelectedHook.Prefix` now checks `PrimalSlot.IsActive` (true for any Primal slot,
not just 666666); `PrimalSlotSetSelectedHook.Prefix` now takes `__instance` (added as a Harmony
parameter, matched by name — the original signature was just `(string series, ref int to)`) so it
can read the currently-mounted real slot and map it through `PrimalSlotFor` before rewriting `to`.
The button itself stays one single "PRIMAL SLOT" toggle rather than becoming three separate
buttons — it now means "the Primal version of whatever real slot I'm on right now," which matches
how Tab already behaves and needs no new UI.

No new reflected surface for GOTCHA #1 in this round — `PrimalSlotFor`/`RealSlotFor` are plain
integer arithmetic on a value (`options.saveSlot`) already treated as safe-to-touch-directly, and
the only other change (`__instance` on `PrimalSlotSetSelectedHook.Prefix`) is a standard Harmony
patch parameter, not a new field/method access.

Pushed all three touched files together — `PrimalSlot.cs` (5,294 bytes),
`Gimmicks/PrimalSlotTabSwitchHook.cs` (54,684 bytes), `Gimmicks/PrimalSlotOptionsMenuHooks.cs`
(9,742 bytes) — verified byte-exact on the first `device_commit_files` attempt for all three.
**None of round 4 or round 5 has been built or tested in-game yet.** Next, once built: (1) from
each of the three real slots in turn, Tab into Primal and confirm it lands on that slot's own
distinct Primal save (666666/666667/666668) — e.g. put different characters/progress on each
slot's Primal save and confirm they don't collide or overwrite each other; (2) Tab back out from
each Primal slot and confirm it returns to the correct originating real slot, not always slot 0;
(3) character preservation (round 4) still works correctly per slot, in both directions, for more
than one character; (4) in the Options menu, confirm the "PRIMAL SLOT" button, clicked from real
slot 1 vs. 2 vs. 3, mounts that slot's own Primal save (not always 666666), and that it shows as
selected (highlighted) whenever any Primal slot is active, regardless of which one; (5) the round-3
slide transition still plays correctly on every switch; (6) check `BepInEx/LogOutput.log` for any
`PrimalSlotSwitcher` or `PrimalSlot` line — a failed reflection lookup or unexpected slot number
would show up there first.

## Threat Slowdown and Threat Panic gimmicks, in detail

**History, briefly (the full original trace of both gimmicks' origins is preserved below this
summary in case the reasoning behind the shared threat lookup/threshold is needed again):**
ThreatSlowdown originally implemented a percentage-based movement/climbing/swimming/throwing
debuff, and ThreatPanic originally implemented a per-tick probabilistic wipe of the player's real
input. Both were tested in-game (see the "false alarm" note preserved below), and once the input-
wipe mechanic ("ThreatPanic") was confirmed triggering correctly, the user asked to **swap what
each name means**: "rename the gimmick that drops inputs on threat to threat slowdown and make the
threat panic throw random inputs." Read literally plus the specific design spec that followed
(quoted in full under "Threat Panic, current behavior" below), this means:
- **"ThreatSlowdown" now means the input-drop mechanic** (unchanged in every mechanical detail from
  its original "ThreatPanic" implementation) — losing control outright reads more like grinding to
  a halt than panicking.
- **"ThreatPanic" now means a brand-new mechanic**: involuntary random inputs firing on top of (not
  instead of) the player's real input — the old percentage-stat-debuff mechanic is retired, not
  preserved under a third name.

**Implementation choice, important:** both gimmicks' **IDs were deliberately left unchanged**
(still literally `"ThreatSlowdown"` and `"ThreatPanic"` in `GimmickRegistry.cs`) — only the CODE
inside `ThreatSlowdownGimmick.cs` and `ThreatPanicGimmick.cs` was swapped between the two files.
This was a deliberate choice over renaming the IDs/classes themselves: a `Configurable<bool>`'s
persisted Remix config key is `characterID + "_" + gimmickID`, so changing an ID would have reset
every character's existing on/off assignment for that gimmick, including Monk's (who already had
both flipped on from earlier testing) — swapping the code instead means Monk's existing assignments
transparently pick up the new behavior with nothing to reassign.

### Threat Slowdown, current behavior

Every tick, there's a chance of that tick's already-assembled real input being wiped entirely:
`panicFraction = Clamp01(rawThreat / ThreatLevelOneThreshold)` (0.22, see "Where the threat number
and 0.22 threshold come from" below) is that chance, 0% at no threat, ramping to 100% (permanent
lockout) right at threat level 1. This is a straight relocation of the file's own original
mechanism (see history above) into the `ThreatSlowdownGimmick.cs`/`"ThreatSlowdown"` file/ID, with
zero mechanical changes: same Postfix on `Player.checkInput()`, same fields zeroed (`x`/`y`,
`analogueDir`, `jmp`/`thrw`/`pckp`/`mp`/`spec`/`crouchToggle`, `downDiagonal` — everything except
`gamePad`/`controllerType`, which describe the input device rather than input state), same
struct-in-array reflection mechanics (`Player.input` is `InputPackage[]`; `array.GetValue(0)` boxes
a COPY, so the modified copy must be written back with `array.SetValue(package, 0)` or the change is
silently lost).

### Threat Panic, current behavior

Requested (2026-09-13), immediately after confirming the rename made sense: "change it so that as
we approach .22, monk starts panicing... take away more and more control from the player,
disallowing input" was the OLD spec (now describing ThreatSlowdown); the actual new spec for
"Threat Panic" was given right after: "make the threat panic throw random inputs. logic should go
like this - choose input between 4 directional grab throw jump, and then keep it for 1-10 frames.
Frequency sets how often these happen. Allow player to always input. Inputs can overlap."

Implemented exactly as specified, in `ThreatPanicGimmick.cs`:
- **Seven equally-likely choices** (`PanicKind` enum): Up, Down, Left, Right ("4 directional"),
  Grab, Throw, Jump — one is picked uniformly at random (`UnityEngine.Random.Range(0, 7)`) each
  time a new forced input starts.
- **Held for 1-10 ticks**: `UnityEngine.Random.Range(1, 11)` per instance, decremented every tick,
  removed once it hits zero.
- **Frequency, concretely**: a new per-character Remix setting (`PanicSettings.Frequency`, default
  3, range 1-20, via the same `Configurable<int>`/`OpUpdown` pattern every other gimmick's settings
  sub-view uses — see TimeLimitSettings in TimeLimitGimmick.cs for the established shape) means
  "roughly this many forced inputs start per second, AT threat level 1." Since the game simulates at
  a fixed 40 ticks/second (see TimeLimitGimmick.cs's own `TicksPerSecond` constant/doc comment), the
  actual per-tick trigger chance is `(Frequency / 40f) * panicFraction` — scaling down linearly as
  threat eases below level 1, same ramp every other Threat gimmick uses. Phrased as "per second" so
  a bigger number in the Remix UI intuitively means MORE frequent panic, not less.
- **"Allow player to always input", taken literally**: unlike ThreatSlowdown (which wipes
  everything), this Postfix never clears or overwrites a real press. `jmp`/`thrw`/`pckp` are OR'd —
  a forced action can only ever ADD a press that wasn't there, never remove a real one. Directions
  are the one exception: forcing `x` or `y` necessarily overwrites whatever the player's own input
  was on THAT axis for the duration (there's no way to "add" a second value onto an already-
  single-valued int) — but only the specific axis a currently-active direction targets is touched;
  the other axis and every other field are left exactly as real input produced them.
  `InputPackage.analogueDir` (the Vector2 an analog stick actually reports) is kept in sync on
  whichever axis is forced too, by symmetry with how the old/ThreatSlowdown mechanic zeroed it for
  the same reason (a judgment call, not specifically requested — worth checking in-game since
  keyboard-only testing wouldn't reveal a problem either way).
- **"Inputs can overlap"**: multiple forced inputs can be active at once, tracked per-player as a
  `List<PanicInstance>` (`ConditionalWeakTable<Player, List<PanicInstance>>`, keyed on `Player`
  itself — a reference type, unlike the value-type `SlugcatStats` key the OLD ThreatSlowdown
  baseline-cache used to use, which is gone now along with the rest of that retired mechanic). A new
  forced input starting doesn't cancel or replace ones already in flight; all currently-active
  instances are applied together every tick. If two active instances disagree on the same axis at
  once (e.g. a still-active Left and a newly-triggered Right), the more-recently-triggered one wins
  for that tick — a simple, deterministic tie-break (applied in list order, later entries overwrite
  earlier ones on the same axis), not an attempt to model "real" conflicting keypresses.

**Known, not specifically resolved interaction**: both gimmicks Postfix the exact same method
(`Player.checkInput`), and Harmony doesn't guarantee which one runs first when a character has both
enabled (true for Monk, currently). If both happen to trigger the same tick, whichever runs second
wins that tick (a forced input either gets asserted then wiped by the drop, or asserted after the
wipe and survives) — a same-tick coin flip between two already-probabilistic effects, not a crash,
but worth knowing about if testing with both on at once looks inconsistent on any single tick.

**Files, this round**: `ThreatSlowdownGimmick.cs` (17,083 bytes) and `ThreatPanicGimmick.cs`
(17,852 bytes) both fully rewritten (code swapped between them, not just renamed);
`GimmickRegistry.cs` (12,716 bytes) updated — both `GimmickDef` descriptions rewritten to match, and
`ThreatPanic`'s entry now wires up `PanicSettings.Bind`/`PanicSettings.BuildUI` (it had no settings
before). `GimmickDefaults.cs` untouched — Monk's existing default assignment
(`GimmickRegistry.ThreatPanic.ID` → true) needed no code change since the ID didn't change, and now
correctly grants Monk the NEW random-input mechanic instead of the old input-wipe one. All three
touched files pushed via `device_commit_files`; the Bash tool's safety classifier was intermittently
unavailable in this container during this round, so the usual local `wc -c`-before/`device_list_dir`
-after byte comparison couldn't be completed as rigorously as the established workflow calls for —
`device_list_dir` afterward did show each file's size and mtime change to new, plausible values
(not staying at their old numbers, which is what the historical silent-write bug looks like), so the
push very likely succeeded, but this round's verification is weaker than usual and worth an extra
look if anything about these two gimmicks seems off after rebuilding. **Not yet built or tested.**
Next: `dotnet build`, then in-game on Monk confirm (1) input still drops out entirely with rising
frequency under "Threat Slowdown" exactly as it did before the rename (nothing should feel different
there); (2) "Threat Panic" now visibly injects extra directional/grab/throw/jump presses the player
isn't making, roughly `Frequency` times a second at threat level 1, tapering off below that; (3) the
player's own real input keeps working throughout — pressing a direction, jumping, throwing, grabbing
should never feel blocked, only occasionally "joined" by an extra involuntary action; (4) multiple
forced inputs can visibly stack (e.g. a forced direction AND a forced jump at once); (5) whether the
`analogueDir` sync actually matters (controller vs. keyboard); (6) check `BepInEx/LogOutput.log` for
any `ThreatSlowdownGimmick`/`ThreatPanicGimmick` line.

### Where the threat number and 0.22 threshold come from (unchanged by the rename)

**What "Threat Music Pulse" actually is**, found by disassembling `HUD.InitSinglePlayerHud` (the
method that adds every HUD element) fresh rather than guessing at a name: a Remix option,
`ModManager.MMF && MMF.cfgThreatMusicPulse.Value`, gating whether a `MoreSlugcats.ThreatPulser` HUD
part is ever constructed — a row of 5 `HUDCircle`s (a hardcoded array size, confirmed in
`ThreatPulser`'s own constructor: `new HUDCircle[5]`) that fill in one at a time as threat rises.
That checkbox only controls whether this HUD element is ever drawn; it has no bearing on whether
the underlying threat number exists, so both gimmicks read that raw value directly and work
regardless of whether the player has this visual cue turned on.

**Where the raw threat value comes from**: disassembled `ThreatPulser.get_Threat()` itself (the
property that HUD element reads every frame) rather than guessing. For ordinary story-mode play
with a live `MusicPlayer` (the normal case): `ProcessManager.musicPlayer.threatTracker
.currentMusicAgnosticThreat` (`Music.PlayerThreatTracker`, a float, updated every frame by that
class's own `Update()`). Vanilla's own fallback, for whatever edge case has no live MusicPlayer:
`ProcessManager.fallbackThreatDetermination.currentMusicAgnosticThreat` (`ThreatDetermination`,
which vanilla lazily constructs if missing — this gimmick does NOT construct it itself if missing,
since side-effecting `ProcessManager` state isn't its job; that edge case just reads as 0 instead).
"MusicAgnostic" here just means "the same number that picks the danger-music track, without
whatever the music mix itself does to it" — exactly what the visual pulse displays. Both
`ThreatLevelOneThreshold` and this lookup (`GetCurrentThreat`) live in `ThreatSlowdownGimmick.cs` as
`internal` members specifically so `ThreatPanicGimmick.cs` shares them instead of re-deriving its
own copy — the two gimmicks are two faces of the same fact, and letting them drift apart if retuned
later would be a real risk.

**Pinning down "threat level 1" as an actual number**: the user's own description ("one full circle
at level 1, two full circles at level 2") is exactly `ThreatPulser.Update()`'s own rendering math,
disassembled to find the real threshold: `Mathf.InverseLerp(0f, 1.1f, rawThreat) * circles.Length`
(5) decides how many of the 5 circles are lit. One full circle lit means that expression equals 1,
i.e. `rawThreat == 1.1f / 5f == 0.22`. Both `1.1` and `5` are vanilla's own literal constants baked
into that one rendering method — a real derived threshold, not an eyeballed guess.

GOTCHA #1 (both files): `Player.input` (a property), every `Player.InputPackage` field touched,
`ProcessManager.musicPlayer`, `Music.MusicPlayer.threatTracker`,
`Music.PlayerThreatTracker.currentMusicAgnosticThreat`, `ProcessManager.fallbackThreatDetermination`,
and `ThreatDetermination.currentMusicAgnosticThreat` are all genuinely new surface, so every one is
resolved via `AccessTools`/`FieldInfo` rather than direct access. `Player.InputPackage` itself, used
directly as a compile-time type, is trusted without reflection on the same nested-type precedent
already established elsewhere (`Player.NPCStats` in SlugpupGimmick.cs, `Menu.SlugcatSelectMenu
.SlugcatPage` in PrimalSlotTabSwitchHook.cs). The `Configurable<int>`/`OpUpdown` settings machinery
`PanicSettings` uses is Remix's own modder-facing UI API, not gated by GOTCHA #1 at all.

<details>
<summary>Original trace, preserved for history (both gimmicks' pre-rename designs — the reasoning
above already carries forward everything still relevant; this is kept only in case a future session
needs the original request wording or the now-retired stat-debuff mechanic's exact numbers)</summary>

**ThreatSlowdown, originally** (requested 2026-09-13): "in remix mod settings, there is a feature of
(threat music visual pulse) which visually shows a threat meter. Id like to add a functionality that
as the threat pulse increase, the character slows down... Lets make it so that character stats
change by percent of how far you are towards threat level 1 - at no threat, you have 100% movement,
climbing, throwing, so on stats - at threat level 1, they go to 0%." Implemented as: `debuffFraction
= Clamp01(rawThreat / 0.22)`, `statMultiplier = 1 - debuffFraction` applied to `SlugcatStats
.runspeedFac`/`poleClimbSpeedFac`/`corridorClimbSpeedFac`/`swimForceFac`; `throwingSkill` (an int
where, per `Player.ThrowObject`'s disassembly, 0 is vanilla's BEST accuracy) instead got an ADDED
penalty, `baseThrowingSkill + RoundToInt(3 * debuffFraction)`, since multiplying it toward 0 would
have made throws MORE accurate — backwards. Required baseline caching (a `ConditionalWeakTable
<SlugcatStats, Baseline>`) since `Player.slugcatStats` is one object shared for the whole session,
mutated in place every tick — each tick's new value had to come from a frozen first-seen snapshot,
never from the object's own already-debuffed current value, or the stat would ratchet down forever.
Used a Prefix on `Player.Update(bool)` (not a Postfix) since `Player.MovementUpdate` — which reads
`corridorClimbSpeedFac` — is called partway through that same method, so a Postfix would be one tick
stale. **Tested in-game 2026-09-13**: user assigned it to Monk and reported "no effect even at 2.5
circles filled" — before assuming a bug, asked whether they'd switched onto Primal's reserved slot
first (every gimmick in this mod gates on `PrimalSlot.IsActive`). They hadn't: "ok, my bad, it did
work, i was not on primal slot." Confirmed the gate and mechanism worked; the exact feel was never
specifically commented on before the whole mechanic was retired in favor of the rename above.

**ThreatPanic, originally** (requested 2026-09-13, right after the false alarm above was resolved):
"lets change the mechanic now that the trigger is working - change it so that as we approach .22,
monk starts panicing - look into slugpup scared/panicking ai and as it approaches .22, take away
more and more control from the player, disallowing input. call it threat panic gimmick, add it to
monk and keep threat slow gimmick for now." Investigated slugpup/creature "panic" AI first and found
nothing reusable — every `panic`/`fear`/`Flee` field in the assembly (`LizardAI.panic`, `ScavengerAI
.scared`, `BarnacleAI.panic`, etc.) belongs to non-player creature AI driving autonomous pathing, not
applicable to a human-steered character. Considered and rejected `Player.controller`
(`Player.PlayerController` — an all-or-nothing input-source override with no partial/graduated
version) and `Player.AI` (`ArtificialIntelligence` — a full autonomous-pathing system, too heavy and
not player-directed at all) as the mechanism, since neither offers a graduated "more and more"
version of losing control. Implemented instead as a Postfix on `Player.checkInput()` that
probabilistically wiped that tick's already-assembled real input entirely — `panicFraction =
Clamp01(rawThreat / 0.22)` was the wipe chance, 0% at no threat ramping to 100% at threat level 1.
**Tested in-game, confirmed working** — user reported "it works" (in the same message that also
asked for the add-gimmick picker scrollbar fix) before pivoting the mechanic to what "Threat Panic"
now means; see the sections above for the current design.
</details>

## Config menu UI, in detail

Requested by the user once Spearmaster's three gimmicks were in place: the original Remix screen
was a flat grid, one `OpCheckBox` per (character, gimmick) pair. That works for a handful of
gimmicks but "falls apart with many gimmicks (expect there to be around 15)" (user's own words) —
both because the grid gets wide, and because a checkbox has no room for a gimmick's own settings
(e.g. TimeLimit's minutes). Replaced with a four-view drill-down (character list → that character's
assigned-gimmick table → one gimmick's settings, or an add-gimmick picker), where selecting a
character used to swap the *entire* screen away from the character list.

**Then changed again (2026-09-11), at the user's explicit request:** the character list shouldn't
disappear when you pick a character — "the gimmick list for a character appears below the character
select screen without actually removing the character select... you don't go to a new window, you
append to the character select window... switching characters overwrites that part of the window."
Current design, all inside `PrimalOptions`:

- **The character grid is permanent.** Built once in `Initialize()` via `BuildCharacterGrid()` and
  added straight to the `OpTab` (not through the wipeable `_detailItems` list) alongside the title
  and subtitle labels — none of the three are ever removed or rebuilt for the rest of the menu's
  life. Same layout as before: a button per real vanilla character (`OpSimpleButton`, the
  `AllCharacters` array), 5 columns x 2 rows (see "Screen is ~600x600, not 1000+" below for why 5x2
  specifically). **`AllCharacters` (top of `PrimalOptions.cs`, just past the `CharacterEntry`
  struct) is where each character's label *inside this mod's own settings screen* comes from** —
  e.g. `new CharacterEntry(MoreSlugcatsEnums.SlugcatStatsName.Artificer, "Artificer")`. That's a
  different, unrelated string from the alt-slot in-game display name (see "Primal character names,
  in detail" for that one) — this one only ever shows up on the grid button and every "X on
  Artificer"/"Edit gimmicks assigned to Artificer" string built from `character.DisplayName`
  elsewhere in this same file. **Update (2026-09-12): added a 10th entry, "Inv"** —
  `MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel` (More Slugcats' secret/non-canon slugcat; "Inv" is
  what the community calls it everywhere, "Sofanthiel" is the actual internal field name, confirmed
  via the real game assembly's metadata — not a guess). 10 characters still fits the 5x2 grid exactly
  (5 columns × 2 rows = 10 slots, now all full, no layout change needed).
- **The detail area is everything below the grid**, tracked in `_detailItems` and wiped/rebuilt via
  `ShowDetail()` (renamed from the old `ShowDynamic()`) every time it changes — this is the *only*
  thing that ever gets removed/re-added; the grid above it is untouched by any of this. It starts at
  `DetailTopY` (335), comfortably below the grid's lowest row (whose buttons bottom out at 365, i.e.
  `GridStartY + GridSpacingY - GridButtonH` = 460 − 55 − 40). Before any character has been clicked,
  it just shows a one-line placeholder ("Select a character above...").
- Clicking a character button (`OpenCharacter`) always jumps the detail area straight to that
  character's gimmick table — **overwriting whatever the detail area was previously showing**,
  including if it was mid-way through a *different* character's settings or add-gimmick picker.
  This is the "switching characters overwrites that part of the window" behavior the user asked for.
- Within the detail area, the same three sub-views as before still exist, now reached without ever
  hiding the grid:
  1. **Gimmicks** (`RenderCharacterGimmicks`) — one row per gimmick *currently assigned* to the
     selected character: a name button spanning most of the content width (click →
     `OpenGimmickSettings`, sub-view 2) plus a small "x" button at the row's right edge (click →
     `RemoveGimmick`, unassigns and re-renders this same sub-view); a placeholder label if none are
     assigned; a "+" button in its own column immediately to the right of the table, spanning the
     same vertical range as the table's rows (click → `OpenAddGimmick`, sub-view 3). No "back"
     button here anymore — there's nothing to go back to; the grid above already lets you pick a
     different character directly.
  2. **Settings** (`RenderGimmickSettings`) — a "< CharacterName" back button (→ `OpenCharacter`,
     back to sub-view 1 for the same character); if the gimmick has settings
     (`GimmickDef.HasSettings`), renders `gimmick.BuildSettingsUI(characterID, topY)`; otherwise a
     plain "This gimmick has no additional settings" label.
  3. **AddGimmick** (`RenderAddGimmick`) — a "< CharacterName" back button (→ `OpenCharacter`); a
     button per gimmick *not yet assigned* to that character (click → `AddGimmick`, assigns it and
     drops back to sub-view 1); a placeholder label if all are already assigned.

**Tested by the user (2026-09-11) — did not work as designed.** The user reports: "the ui works,
but stayed the same - on clicking a character, the character selection disappeared and coming back
brought it back, like the previous version of the ui." I.e. it compiled and ran (this session's
container had no `dotnet`/`mono` to verify the build itself — see "Container environment notes"
below — so the user must have built it), but the actual on-screen behavior matched the *old*
drill-down design, not the merged grid-plus-detail-panel design described above. **The user asked
to leave this alone rather than keep debugging it** — do not resume without being asked to.

Leaving a hypothesis for whoever picks this back up, **unconfirmed**: the reported symptom ("coming
back brought it back") sounds like a back-navigation flow, but the merged design deliberately has
**no back button** in the Gimmicks sub-view (see the numbered list above) — there's nothing to
navigate "back" from since the grid was supposed to already be visible. That mismatch is consistent
with the user having tested a **stale build** (e.g. the installed
`.../StreamingAssets/mods/Primal/plugins/Primal.dll` not actually replaced with a
DLL built from the new source, or a build that picked up the old `.cs` from a different output
path) rather than a logic bug in the new source itself — but this is a guess, not a confirmed root
cause, and the alternative (a real bug in the new `PrimalOptions.cs` that happens to produce
identical-looking behavior to the old design) hasn't been ruled out either. Don't assume the
hypothesis is right; verify from scratch if this is picked up again — confirm what DLL/source the
user actually tested before re-diagnosing.

### Screen is ~600x600, not 1000+ (layout correction, from the first real test)

The very first layout pass (character grid: 3 columns, `spacingX = 260`, `startX = 20`) assumed a
screen roughly 1000+ units wide, by eye against the old flat grid's own constants. **Wrong** — the
user's first screenshot of the actual in-game menu showed the third column's buttons (Hunter, and
the rest of that row) rendering well past the panel's right edge, clipped by the visible window.
Confirmed via disassembly of the game's own `MoreSlugcatsOptionInterface` (a real, shipped Remix
screen) that its own elements never sit outside roughly a 0–600 coordinate range on either axis —
this mod's screen (`TitleY = 570`, elements as far right as `x = 580`) was already implicitly
relying on that same ~600x600 space for its vertical axis; the character grid's horizontal layout
just hadn't been checked against it. Fixed by moving every view's horizontal layout onto a shared
`ContentLeft`/`ContentWidth` (10 / 580) convention in `PrimalOptions.cs`, and changing the
character grid to 5 columns x 2 rows (`buttonW = 106`, tight enough that 5 fit inside 580 with
10px gaps) instead of 3 columns x 3 rows sized for a screen that doesn't exist. **If any future
element still renders clipped/off-panel, assume the ~600x600 budget again before adding width.**

**Mechanics (current, post-merge):** one `OpTab` is created once in `Initialize()` and never
replaced. The title, subtitle, and all 9 character-grid buttons are added straight to the tab once
and never touched again — they are permanent, not tracked in any wipeable list. A separate
`List<UIelement> _detailItems` (renamed from the pre-merge `_dynamicItems`) tracks only whatever
the *detail area below the grid* currently shows; changing the selected character or drilling into
a settings/add-gimmick sub-view calls `_tab.RemoveItems(_detailItems.ToArray())`, clears the list,
builds the new sub-view's elements, adds them via `_tab.AddItems(...)`, and refills `_detailItems`
— the grid and title/subtitle above are never part of this and are therefore never removed.
`OpTab.AddItems`/`RemoveItems` are public instance methods (confirmed via decompilation of the
Remix `Menu.Remix.MixedUI` framework) that can be called repeatedly after `Initialize()` — this is
what makes a persistent-grid-plus-swappable-detail-panel layout possible on a single Remix tab.

**Every (character, gimmick) enabled `Configurable<bool>` is still bound eagerly for every pair in
`Initialize()`**, exactly like the old flat-grid version (`config.Bind(character.ID + "_" +
gimmick.ID, ...)`, then `GimmickAssignments.Register(...)`) — nothing changed about how
`GimmickAssignments.IsEnabled` works or when its backing data exists. `PrimalOptions` also
keeps its own `Dictionary<string, Configurable<bool>>` (`_enabledConfigs`, keyed the same way as
`GimmickAssignments`'s internal one) purely so the drill-down views can flip a pair's value
directly (the "x" button sets it false, the add-gimmick picker sets it true) without needing new
lookup surface added to `GimmickAssignments` itself.

**The `GimmickDef.BindSettings`/`BuildSettingsUI` extensibility pattern** (see `GimmickDef.cs`'s
own doc comments): a gimmick with settings beyond plain enabled/disabled supplies both — a
`Static class FooSettings` sibling to the gimmick's own Harmony patch class, following the exact
shape already established by `TimeLimitSettings`, `NoSpearsSettings`, and `OneSpearSettings` (all
three now exist, one per file, next to their gimmick's patch class):
- a `Dictionary<string /*characterID*/, Configurable<T>>` per setting value,
- `Bind(OptionInterface owner, string characterID)` — called once per character in
  `PrimalOptions.Initialize()` (via `gimmick.BindSettings?.Invoke(this, character.ID)`,
  right alongside the enabled-flag binding, so it happens unconditionally regardless of whether
  that character currently has the gimmick assigned — same "bind everything up front" convention),
- a getter (`GetMinutes`, `GetLimit`, `IsAllowed`, etc.) the gimmick's own Harmony patch reads at
  runtime,
- `BuildUI(string characterID, float topY) -> List<UIelement>` — lays out this setting's controls
  downward from `topY` **the first time** it's called for a given character, then returns that
  same cached `List<UIelement>` (and the same element instances inside it) on every later call for
  that character — see **Gotcha #4** for why rebuilding fresh elements on every call crashes the
  menu the second time a settings sub-view is revisited.

Current settings, concretely:
- **TimeLimit** (`TimeLimitSettings`): `Configurable<int> minutes` (default 60, range 1–999, via
  `OpUpdown` + `ConfigAcceptableRange<int>`) and `Configurable<bool> perCycle` (default false, via
  `OpCheckBox`) — "measure per cycle instead of per run".
- **NoSpears** (`NoSpearsSettings`): three independent `Configurable<bool>` toggles (plain /
  explosive / electric spear types), each default false (not allowed) — matching the gimmick's
  original blanket-block behavior; checking one re-allows that specific spear type.
- **OneSpear/"Master Spear"** (`OneSpearSettings`): `Configurable<int> limit` (default 1, range
  1–50, via `OpUpdown`) — lifetime pull count. Required changing `OneSpearGimmick.cs`'s save-marker
  scheme from a single un-indexed marker to indexed-per-use markers so a *count* (not just a
  boolean) can be tracked — see that file's own doc comment and the file-map entry above for the
  backward-compatibility note.
- **Misery**: no settings (`BindSettings`/`BuildSettingsUI` both left null in `GimmickRegistry.cs`)
  — nothing obvious to expose yet. If asked for ideas later: could expose the exhaustion multiplier
  or which of the two effects (instant-max-exhaustion vs. forced-PainJumps) are active.
- **Slugpup** (`SlugpupEndingSettings`, added 2026-09-15): `Configurable<bool>
  EndsOnBeatingGame` (default **true**, via `OpCheckBox`) — whether beating the game via any
  standard (non-ascension) ending permanently stops the gimmick for that character on that save
  file. See "Slugpup gimmick: ends at Outer Expanse, in detail" below.

**First real in-game test result:** the user opened the drill-down menu (the design that predates
the character-grid/detail-panel merge above) in Remix and confirmed the core navigation worked —
`OpSimpleButton`'s constructor/event signature and `OpTab.AddItems`/`RemoveItems` all behaved as
decompilation predicted; every view opened successfully the first time through. Two real bugs
surfaced from that same test, both described in detail above:
1. The character grid's 3-column layout rendered past the panel's right edge — see "Screen is
   ~600x600, not 1000+" above. **Fixed and confirmed resolved by the user** (5x2 layout).
2. Opening the *same* gimmick's settings sub-view a second time crashed the whole `OptionInterface`
   with `Menu.Remix.MultiuseConfigurableException` — see **Gotcha #4** above. **Fixed and confirmed
   resolved by the user.**

Both fixes shipped as part of the drill-down design; the drill-down design itself was then replaced
by the character-grid/detail-panel merge described earlier in this section, which reuses the same
fixes (5x2 grid, cached settings elements) and is the one still awaiting build verification and an
in-game test — see "Outstanding items" below.

**Update (2026-09-13): the character-grid/detail-panel merge is confirmed working in-game** —
this came out incidentally while the user was testing Threat Panic ("it works, but add a
scrollwheel to the gimmick picking list, because you cant see all of them at this point"), which
means the grid-plus-detail-panel design (including drilling into a character, opening the
add-gimmick picker, and assigning a gimmick from it) was actually exercised successfully. The
stale "left alone at the user's request, don't resume debugging" note above refers to an *earlier*
report that has since been superseded — this whole screen is live and working, just needed the fix
below.

### Add-gimmick picker made scrollable (2026-09-13)

With 12 gimmicks now registered (see Threat Slowdown / Threat Panic above), `RenderAddGimmick`'s
list of not-yet-assigned gimmicks could need up to 12 rows for a character with few/none assigned
— 12 × `RowHeight` (40) = 480px, far taller than the ~300px actually available in the detail area
below the character grid (see "Screen is ~600x600, not 1000+" above). Rows past the visible area
were simply unreachable — exactly the "you cant see all of them" report.

Fixed using the game's own scrollable-list element, `Menu.Remix.MixedUI.OpScrollBox` — found and
its real usage confirmed by disassembling the one place the base game itself uses it,
`OptionInterface.ErrorScreen` (located via a raw IL search across the whole assembly for calls to
`OpScrollBox`'s constructor, since nothing in this project had ever used it before). Same
namespace as `OpTab`/`OpLabel`/`OpSimpleButton`/`Configurable<T>` — Remix's own modder-facing UI
API, already used directly (no reflection) everywhere else in `PrimalOptions.cs` — so `OpScrollBox`
is used directly too, not through `AccessTools` (Gotcha #1 is about the game's own *gameplay*
classes being falsely-public in the compile-time reference assembly; Remix's UI classes are the
real, intentionally-public modding API and aren't affected by that gotcha).

Mechanics, concretely: `OpScrollBox`'s constructor is `(Vector2 pos, Vector2 size, float
contentSize, bool horizontal, bool hasBack, bool hasSlideBar)` — `pos`/`size` are the box's fixed
on-screen viewport (sized to the detail area's actual available space, `AddGimmickListWidth` x
`AddGimmickListHeight`), and `contentSize` is the full *virtual* height of everything added to it
(`candidates.Count * RowHeight`, clamped to at least the viewport height) — independent of, and
here taller than, the viewport, which is what makes it scroll. Row buttons are added via the scroll
box's *own* `AddItems` (not the `OpTab`'s), positioned in coordinates local to that virtual content
height rather than against `DetailTopY` — same bottom-left-origin/Y-increases-upward convention as
every other view in this file, just measured against `contentSize`. The scroll box itself is added
to `_detailItems` like any other element, so it's correctly torn down/rebuilt by `ShowDetail()`
whenever the character or sub-view changes, same as before.

Only `RenderAddGimmick` (the "+" picker) was changed. `RenderCharacterGimmicks` (the table of a
character's *already-assigned* gimmicks) is not yet wrapped in a scroll box — with all 12 gimmicks
now possible on one character, that table could eventually overflow the same way, but hasn't been
reported as an actual problem yet; worth revisiting if it comes up (see "Outstanding items" below).

**First attempt crashed on the very first open, fixed same day.** The user rebuilt and opened the
add-gimmick picker and got an immediate `OptionInterface` error screen:
`System.InvalidOperationException: OpScrollBox must be added to an OpTab before items are added.`
— thrown from inside `OpScrollBox.AddItems` itself, called from `RenderAddGimmick`. Root cause:
`RenderAddGimmick` had built the *entire* detail area (back button, heading, the still-empty
`OpScrollBox`, and finally that scroll box's own row buttons) into one `items` list and committed
all of it in a single `ShowDetail(items)` call at the very end — the same one-pass-then-commit
shape every other sub-view in this file uses. But `OpScrollBox.AddItems` (called on the scroll box
directly, to populate its rows) apparently requires the scroll box to *already* be attached to the
`OpTab` first — calling it beforehand throws immediately, every time, unconditionally. This wasn't
predictable from the disassembled `ErrorScreen` reference alone (that method happens to call
`tab.AddItems([scrollBox])` before `scrollBox.AddItems([...])` too, but nothing in the earlier trace
had flagged *why* that ordering mattered, or that reversing it would throw outright rather than just
misbehave) — confirmed as a hard requirement only once the user hit it in-game.

**Fix:** split `RenderAddGimmick` into two commits instead of one — `ShowDetail(items)` is now
called right after the *empty* scroll box is added to the `items` list (attaching it, and
everything else in that list, to the `OpTab`), and only afterward are the scroll box's own row
buttons built and added via `scrollBox.AddItems(...)`. Every other sub-view in this file is
unaffected — this two-step-commit requirement is specific to `OpScrollBox` itself, not a general
`ShowDetail`/`OpTab` issue.

Pushed `Gimmicks/PrimalOptions.cs` again (37,882 bytes) and verified byte-exact on the first
attempt. **Not yet rebuilt or re-tested by the user** — worth confirming: the picker opens without
the error screen this time; the list actually scrolls (mouse wheel and/or the slide bar) instead of
just being clipped a different way; every gimmick in the list is reachable and clickable, including
the last one; and the rest of the Remix screen (character grid, assigned-gimmick table, settings
sub-views) still look and behave exactly as before.

### Per-gimmick settings sub-view made scrollable too (2026-09-15)

Reported directly: some gimmicks' *settings* sub-view (`RenderGimmickSettings`, opened by clicking
a gimmick's name in a character's assigned-gimmick table — distinct from the add-gimmick picker
above) don't fit the available space either, "notably Artificer's applied gimmick of Chains."
Checked against `ChainsSettings.BuildUI` (`ChainsGimmick.cs`): its content (an escape-timer row,
two checkboxes, a "Quick preset" `OpListBox`, then a 6-row/2-column, 12-checkbox banlist grid) lays
out from `topY` down to roughly `topY - 424` — well past the ~241px actually available below the
heading (`DetailTopY - 84f` down to a small bottom margin, the same space `RenderGimmickSettings`
used to place settings content into directly, unwrapped). Everything past that point rendered off
the bottom of the Remix window with no way to reach it — the exact same class of bug the add-gimmick
picker had (see above), just in the sibling sub-view that fix didn't touch.

**Fix:** `RenderGimmickSettings` now wraps whatever `GimmickDef.BuildSettingsUI` returns in an
`OpScrollBox`, same pattern as `RenderAddGimmick` (including the same two-step-commit ordering —
`ShowDetail(items)` with the still-empty scroll box first, then `scrollBox.AddItems(...)`
afterward). The one wrinkle settings has that the add-gimmick picker didn't: that picker always
knows its own content height from a simple formula (`candidates.Count * RowHeight`), but a
gimmick's settings content height is arbitrary and defined entirely inside that gimmick's own
`*Settings.BuildUI` — `GimmickDef` had no way to know it up front. Rather than have
`PrimalOptions` inspect already-built elements' positions to guess a bounding box, `GimmickDef`
gained one new optional field, `SettingsContentHeight` (default `0`), which a gimmick can set to
however tall its own settings content actually is; `RenderGimmickSettings` clamps the scroll box's
content size up to at least the plain viewport height either way
(`Mathf.Max(SettingsViewportHeight, gimmick.SettingsContentHeight)`), so leaving it at the default
is a correct, silent no-op for every gimmick whose settings already fit (TimeLimit, NoSpears,
OneSpear, ThreatPanic) — only `Chains` sets it explicitly, to `480f` (a little headroom over the
~424px actually needed). No changes were needed inside any `*Settings.BuildUI` method itself —
every one of them already treats the `topY` argument as an opaque "lay out downward from here"
starting coordinate (per `GimmickDef.BuildSettingsUI`'s own doc-comment contract), so handing them
a coordinate local to the scroll box's own content area instead of an absolute screen coordinate
needed no code changes on their end, just a different `topY` value computed at the call site.

Touched `GimmickDef.cs` (new `SettingsContentHeight` field + constructor parameter),
`GimmickRegistry.cs` (`Chains`'s registration passes `settingsContentHeight: 480f`), and
`PrimalOptions.cs` (`RenderGimmickSettings` rewritten as described above). **Not built or tested at
all** — this container has no `.NET` toolchain (see "Container environment notes"), so this
couldn't even be locally reasoned about beyond careful reading of the existing, already-working
`RenderAddGimmick` scroll box as a template. When this next reaches a real build+test cycle, check:
(1) it still compiles (three touched files, no new `using`s needed — `List<T>.ToArray()` is a
plain BCL method); (2) opening Chains' settings for Artificer no longer clips — the escape timer,
both toggles, the preset picker, and all 12 banlist checkboxes (both columns) are all reachable via
scrolling and each control still reads/writes the right `Configurable`; (3) every OTHER gimmick's
settings sub-view (TimeLimit, NoSpears, OneSpear, ThreatPanic) still looks and behaves exactly as
before — these should be an invisible no-op wrapper, not a visible layout change, since their
content already fit within the plain viewport height; (4) the settings-viewport's slide bar doesn't
visually collide with anything at the settings screen's right edge (a margin choice, `ContentWidth
- 40f`, not verified in-game any more than the add-gimmick picker's own analogous margin was when
it shipped).

### Gimmick buttons colored by their home character (2026-09-15)

Requested directly: gimmicks are already thematically tied to certain characters by default (Chains
↔ Artificer, ThreatPanic ↔ Monk, etc.), so color-code each gimmick's button in the Remix menu by
that character's own color, using "a variable [on] each gimmick to reference a character" plus "each
character... their own color."

**The character reference:** `GimmickDef` gained a new optional field, `HomeCharacter`
(`SlugcatStats.Name`, default `null`) — a *display-only* tag independent of `GimmickDefaults`' real
default (character, gimmick) assignments (which can differ or cover multiple characters — NoYeeks/
NoCicadas/NoMauling default onto both Artificer AND Inv, for instance) and independent of whatever
a player has actually assigned a gimmick to at runtime. `GimmickRegistry.cs` sets it per gimmick,
matching each one's own doc comment/History account of which character it was designed around:
Misery/Slugpup → Gourmand, OneSpear/TimeLimit/NoSpears → Spearmaster, Chains/NoYeeks/NoCicadas/
NoMauling → Artificer, ThreatSlowdown/ThreatPanic → Monk. `NoPassages` deliberately has none (left
`null`) — it was never requested for, or thematically tied to, any specific character.

**The character's color:** rather than hand-picking new colors, `PrimalOptions.GetHomeCharacterColor`
reuses vanilla's own real per-character palette — `PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name)`
— the same "History" preference this whole project keeps coming back to (ride on an existing,
already-tested vanilla system instead of reimplementing one). Deliberately `DefaultSlugcatColor`,
**not** the "live" `PlayerGraphics.SlugcatColor` already used elsewhere in this codebase
(`SlugpupColorGimmick`, see "Slugpup gimmick stats rescaling, in detail" above) — `SlugcatColor` also
factors in the player's own Jolly Co-op custom/auto color settings, which would make a gimmick's
color tag silently drift based on unrelated Options instead of just reflecting that character's own
identity color; `DefaultSlugcatColor` is a plain, state-free per-character lookup (confirmed via
`reference/GameSource/PlayerGraphics.cs` — Gourmand's tan/pink, Rivulet's blue, Artificer's
maroon, etc., the same literal values already traced in the Slugpup section above for an unrelated
fix). Resolved via `AccessTools.Method` reflection, same Gotcha #1 discipline as every other
gameplay-class member this codebase touches (showing `public` in the compile-time/publicized
reference assembly proves nothing about the real one) — and, since this one runs while the Remix
menu itself is being built rather than in a per-tick gameplay patch, wrapped in its own try/catch
(logs and returns "no color" on failure) so a `MethodAccessException` here can't take the whole
config screen down with Remix's own error screen the way an uncaught one would.

**Where it's applied:** `OpSimpleButton.colorEdge` (confirmed via `reference/GameSource/
Menu.Remix.MixedUI/OpSimpleButton.cs` — a plain public `Color` field that drives both the button's
border AND its label text color in `GrafUpdate`, `_label.color = base.bumpBehav.GetColor(colorEdge)`)
on the gimmick's name button in the assigned-gimmicks table (`RenderCharacterGimmicks`) and on each
row in the add-gimmick picker (`RenderAddGimmick`) — i.e. every place a gimmick shows up as a
selectable/clickable item in the Remix menu. Deliberately not `colorFill` (which would recolor the
whole button as a solid block, risking unreadable text against some characters' lighter colors) —
just the border+text tint is enough to read as a color tag at a glance without hurting legibility.
A gimmick with no `HomeCharacter` (`NoPassages`) or a failed color lookup just keeps
`OpSimpleButton`'s own default `colorEdge` (`MenuColorEffect.rgbMediumGrey`) untouched, same as
every other unstyled button already on this screen ("x" remove buttons, the "+" button, the
character grid) — no separate "neutral color" constant needed.

Touched `GimmickDef.cs` (new `HomeCharacter` field/parameter), `GimmickRegistry.cs` (each
gimmick's `homeCharacter:` argument), and `PrimalOptions.cs` (`GetHomeCharacterColor` plus the two
`colorEdge` assignments). **Not built or tested at all** — same container limitation as the settings
scroll box above (no `.NET` toolchain here); written from decompiled ground-truth for both new
member accesses (`OpSimpleButton.colorEdge`, `PlayerGraphics.DefaultSlugcatColor`) rather than
guessed, but still genuinely unverified. When this next reaches a real build+test cycle, check: (1)
it compiles; (2) opening any character's gimmick table shows each assigned gimmick's name button
tinted (border + text) in its home character's real in-game color — e.g. Chains on Artificer reads
in Artificer's maroon, Misery/Slugpup on Gourmand read in Gourmand's tan/pink; (3) the add-gimmick
picker shows the same tint per row; (4) NoPassages' button (wherever it's assigned) looks completely
unstyled/default, not some fallback gray meant to look intentional; (5) the tinted text is still
legible against the button's own black fill for every character's color, especially any character
whose color skews dark (Spearmaster's dark purple, Sofanthiel/Inv's near-black navy) -- if any read
as too dim, revisit whether some minimum-brightness floor is worth adding, since none was applied
here; (6) check `BepInEx/LogOutput.log` for a `PlayerGraphics.DefaultSlugcatColor threw` line, which
would mean every gimmick fell back to uncolored and the reflection lookup needs a fix.

## Kill List gimmick, in detail (brand new, never built or tested)

Requested 2026-09-15: "creature kill list — once the gimmick is enabled, the game begins tracking
killed creatures across the entire run and your goal becomes to kill every type of creature. for
now the goal will be to track every creature." Explicitly staged as tracking-only for this pass —
no win condition, no reward, no UI enforcing anything — matching how this project has approached
every other multi-round feature (ship the mechanism first, layer behavior on top once it's
confirmed working).

**Building the roster came first, deliberately, before any code.** Per the user's own instruction
("before code changes, output this list of creatures to this chat"), every registered
`CreatureTemplate.Type` was enumerated directly from `reference/GameSource/` rather than guessed —
three files register them (`CreatureTemplate.cs` for the 47 vanilla ones, `DLCSharedEnums.cs` for
13 shared across DLCs, `MoreSlugcatsEnums.cs` for 5 MSC-only, `Watcher/WatcherEnums.cs` for 27
Watcher-only — 92 total). The user then supplied their own curated list of community/wiki names to
track and asked for each to be matched to its internal type, flagging anything confusing rather
than guessing. Most matched cleanly; a handful needed real disambiguation, resolved with actual
ground truth rather than assumption:
- **"Caramel Lizard" → `SpitLizard`, "Strawberry Lizard" → `ZoopLizard`** — not a guess: found the
  literal in-game display-name strings in `MoreSlugcats/MoreSlugcatsCreatures.cs`
  (`creatureNames[(int)DLCSharedEnums.CreatureTemplateType.SpitLizard] = "Caramel Lizards"`, same
  for `ZoopLizard`/"Strawberry Lizards") — these are the game's own official names, not community
  nicknames, and they directly resolved what would otherwise have been an unverifiable guess.
- **"Coalescipede"** — not found anywhere in the game's code, comments, or asset names (grepped the
  whole decompiled source for `coalesc`, zero hits). Asked the user directly rather than guessing;
  confirmed as `AquaCenti` ("Aquapede" in `MoreSlugcatsCreatures.cs`'s own official name — the
  aquatic centipede-like DLC-shared creature).
- **Centipede growth stages** — the user's list named four stages (Infant/Youth/Mature/Overgrown
  Centipede) plus Red Centipede separately, but code only registers three centipede types total
  (`SmallCentipede`, `Centipede`, `RedCentipede`) — confirmed by grepping the whole decompiled
  source for "youth"/"overgrown"/"infant centipede"/"mature centipede" and finding nothing; the
  size gradient within the plain `Centipede` type is purely visual/community-described, not a code
  distinction. Asked the user rather than guessing which stage-name maps to which of the two
  remaining types; resolved by just using all three types as found — `SmallCentipede` → "Small
  Centipede", `Centipede` → "Centipede", `RedCentipede` → "Red Centipede" — dropping the four-stage
  framing entirely rather than forcing an arbitrary mapping.
- **"Slugpup"** — flagged as structurally different from everything else on the list (a slugpup is
  normally a `Player` instance with `SlugCatClass == SlugcatStatsName.Slugpup`, not a
  `CreatureTemplate.Type` at all, per this mod's own `SlugpupGimmick.cs`). The user resolved this
  by simply pointing it at `SlugNPC` instead (MSC's own "Slugcat NPC" type, official name "Slugcat
  NPC"/"Slugcats") — a real, ordinary `CreatureTemplate.Type`, sidestepping the special-case
  entirely. `KillListEntry`/`KillListRegistry` therefore need no special handling for this one.
- **"Chieftain Scavenger" → `ScavengerKing`** — the game's own official name is actually "King
  Scavenger" (confirmed via the same `MoreSlugcatsCreatures.cs` lookup), not "Chieftain" — presented
  to the user as a probable match with the official name flagged, not silently substituted.

**What's deliberately NOT on the list**, confirmed unlisted rather than assumed excluded (the user
never responded to this specific question, and moved on to resolving the ambiguous names instead —
worth re-raising if a future session or the user wants the roster expanded): a handful of MSC/DLC
types (`HunterDaddy`, `FireBug`, `TrainLizard`, `TerrorLongLegs`, `BigJelly`) and nearly all of
Watcher's own roster (`DrillCrab`, `TowerCrab`, `Barnacle`, `SandGrub`, `BigSandGrub`, `BigMoth`,
`SmallMoth`, `BoxWorm`, `FireSprite`, `Rattler`, `SkyWhale`, `ScavengerTemplar`,
`ScavengerDisciple`, `Loach`, `RotLoach`, `BlizzardLizard`, `BasiliskLizard`, `IndigoLizard`,
`PeachLizard`, `Rat`, `Frog`, `Tardigrade`, `GrappleSnake`, `Angler`, `RippleSpider`, `MothGrub`) —
plus the three internal-only "ancestor" types (`Slugcat`, `LizardTemplate`,
`StandardGroundCreature`) confirmed, by checking their actual usages in the decompiled source, to
be generic category markers (`creatureTemplate.TopAncestor().type == CreatureTemplate.Type.X`) that
no real spawned creature is ever actually typed as. Roster at this point: 56 entries.

**Update (2026-09-15, fourth round): `Yeek` and `SlugNPC` ("Slugpup") removed, no reason given.**
Requested plainly: "remove yeek and slugpup from the list." Both were resolved matches from the
original curation pass above (`Yeek` — direct match; `SlugNPC` — the user's own resolution for
"Slugpup", since a real slugpup isn't a `CreatureTemplate.Type` at all), not confusing ones, so
there's nothing to re-derive — just two `KillListEntry` lines removed from `KillListRegistry.All`
and every "56" reference elsewhere in this file/the source updated to match. Roster: 54 entries.

**Update (2026-09-15, fifth round): a real miss caught by the user — `Spider` added as "Small
Spider".** The user pointed out directly: "I also see that your list doesnt include the small
spiders — these dont get tracked by the standard kill counter and have the ability to combine into
a bigger mass of spiders." Checked `Spider.cs` to confirm rather than guess: plain
`CreatureTemplate.Type.Spider` (distinct from the already-tracked `BigSpider`/`SpitterSpider`) is
exactly this — each spider belongs to a `Flock` (a `SpiderMass` subclass), and `Flock.Update()`
periodically finds a nearby spider belonging to a *different* flock and calls `Merge(SpiderMass
otherFlock)`, folding the smaller flock's spiders into the larger one. This is the one entry from
the original 92-type enumeration that fell through the cracks during the very first curation pass
— the user's own list named "Big Spider" and "Spitter Spider" specifically but never gave a name
for the plain "Spider," and it was silently dropped as unmatched instead of being flagged the way
`Coalescipede`/`Strawberry Lizard`/etc. were. Added as `new KillListEntry(CreatureTemplate.Type.
Spider, "Small Spider")`, per the user's own chosen display name. **Final roster: 55 entries** —
see `KillListRegistry.All` in `Gimmicks/CreatureKillListGimmick.cs` for the exact list with display
names.

**Verification (2026-09-15, sixth round, no code change): centipede growth stages, including
Coalescipede.** User asked to double-check that `Small Centipede`, `Centipede`, `Red Centipede`,
`Centiwing`, and `Coalescipede` (`AquaCenti`) all get attributed the same way. Confirmed via
`Centipede.cs`: all five roster entries are the *same* `Centipede` class, distinguished purely by
`creatureTemplate.type` (`Small`/`Red`/`Centiwing`/`AquaCenti` are just bool properties reading that
field) — there is no separate `AquaCenti.cs` or `Centiwing.cs`. `Die()` is a one-line override
(`base.Die()`) for all variants alike, so the `killTag`-read at the top of `Creature.Die()` applies
identically regardless of growth stage. `Violence()` is overridden (shell-falloff/shock-resistance
logic differs per variant) but every code path falls through to an unconditional
`base.Violence(...)` call at the end (`Centipede.cs` ~line 1334) — including `Creature.Violence()`'s
own `SetKillTag((source.owner as Creature).abstractCreature)` when hit by another creature's body
chunk. Spear stab/throw kills are attributed even more directly and universally, regardless of
creature type: `Weapon.cs` (~line 363, the shared thrown/melee-hit collision path) calls
`SetKillTag(thrownBy.abstractCreature)` unconditionally on any `Creature` a spear hits, before
`HitSomething`/`Violence` even run. The one real difference: `Edible => Small` (`Centipede.cs:205`)
— only `SmallCentipede` can be grabbed-and-eaten via the `IPlayerEdible`/`BiteEdibleObject` path;
the larger stages can't be bite-killed at all, matching normal vanilla behavior, not a tracking gap.
Also checked `Centipede.cs:1454` (`(shockObj as Creature).Die()`, no `SetKillTag`) — that's a
centipede *shocking prey to death* (its own predation ability against something else), not a path
by which the centipede itself dies, so it's irrelevant to this gimmick. No `airDrown`-style
direct-`Die()` self-death path (the kind that caused the Leech gap in round 3) exists anywhere in
`Centipede.cs`. Conclusion: all five entries attribute identically; no fix needed, no roster or code
change made this round.

**"Killed by [character]", precisely.** Rather than reimplementing kill attribution from scratch
(fragile territory — Rain World's damage system varies a lot by creature and death cause), this
reuses a mechanism vanilla already has: `Creature.killTag` (an `AbstractCreature`), set by the
attacker's own code, and already read by `Creature.Die()`'s own base implementation at the very
top of its body (for Arena scoring and social-event bookkeeping — confirmed by decompiling both
`SetKillTag` and the start of `Die()`). This means a creature that dies from something other than
direct player action (starvation, drowning, eaten by another creature, environmental hazards) has
no `killTag` pointing at a player and is correctly not counted — a "you killed it" list, not a
"this died somewhere near you" one.

**Why `Creature.Die()` and not `Creature.Violence()`.** `Violence()` looked like the more obvious
patch point at first (it's literally where damage happens), but checking first (this codebase's
usual discipline) found `grep -rl "public override void Violence(" reference/GameSource/` returns
**35 files** — including `Lizard.cs` (covering all 8 lizard color variants), `Scavenger.cs`,
`Vulture.cs`, and most other common creatures — and spot-checking `Lizard.Violence` specifically
found it's a **full reimplementation that never calls `base.Violence(...)`** (different creatures
need completely different damage-resistance math, so each subclass just replaces the whole method).
A Harmony patch on `Creature.Violence` would only ever fire for whichever creatures *don't*
override it — missing most of the tracked roster silently, the exact "looks reasonable, is
actually broken" trap this project's Gotchas exist to catch before shipping. `Die()` overrides, by
contrast, were spot-checked across ~15 of the ~36 files that have one (`Fly`, `BigSpider`,
`Spider`, `Centipede`, `EggBug`, `Snail`, `Vulture`, `TempleGuard`, `DropBug`, `Overseer`, `Leech`,
`TubeWorm`, `Hazer`, `LanternMouse`, `GarbageWorm`, `Deer`, `DaddyLongLegs`, `JetFish`, `Cicada`,
`SmallNeedleWorm`, `BigEel`) and every single one calls `base.Die()` (`Overseer.cs`'s override
does too, just further down its body than the others — also incidentally confirms Overseers really
are killable in vanilla, resolving an earlier open question from when this roster was first being
discussed). One base-class patch on `Die()` therefore gets real, broad coverage; a lizard, a
scavenger, and a vulture all still fire the same Postfix, unlike `Violence()`.

**Attribution, traced in full (2026-09-15, second round — the user asked to "double check
attribution works for stuff like batflies").** Enumerated every `SetKillTag(` call site in the
whole decompiled source rather than trusting the first handful found: **13 total**, across
`Player.cs` (5: a proximity weapon-attack effect, Gourmand's roll attack "SLUGROLLED", the fall-slam
"SLUGSMASH", `BiteEdibleObject` — biting/eating something you're holding — and Artificer's maul),
`Weapon.cs` (2: a thrown weapon's first creature collision, and `HitSomethingWithoutStopping`),
`Explosion.cs`, `Pomegranate.cs`, `FlareBomb.cs`, `SporeCloud.cs`, `LocustSystem.cs`, and
`Watcher/SandGrub.cs`. Separately, `Creature.Violence`'s own **base implementation** (the one ~35
subclasses never call, per above) *also* auto-sets `killTag` whenever the attacking `BodyChunk`'s
owner is itself a `Creature` (`if (source.owner is Creature) SetKillTag(...)`) — a second,
redundant safety net for ordinary creature-vs-creature contact damage.

For batflies (`Fly`, an `IPlayerEdible`): the dominant, iconic way players actually eliminate small
prey in this game — grab it, then eat it — goes through `Player.BiteEdibleObject`, which explicitly
calls `SetKillTag` before consuming. **High confidence this path is correctly attributed.**

**Leeches, double-checked separately (2026-09-15, third round — "double check the leech kill path
too") — a real, confirmed gap, found and fixed.** `Leech.cs` is **not** `IPlayerEdible` (grepped
every `IPlayerEdible` implementer in the whole source; Leech isn't one), so `BiteEdibleObject` never
applies to it at all — there's no "grab and eat" path for leeches the way there is for batflies.
Reading `Leech.cs` end to end for how it actually dies turned up the real mechanism: an `airDrown`
counter (`Leech.Update`, line ~292) that rises while `mainBodyChunk.submersion == 0` (i.e.
out of water) and calls `Die()` **directly, unconditionally, with no `SetKillTag` anywhere in that
code path** once it hits 1.0 (~900 ticks / 22.5 seconds at the increment rate of `0.0011111111f`
per tick). This is the ordinary, everyday way a pulled-off-and-thrown-away leech actually dies in
normal play — not eaten, not smashed, not stabbed — so **the original implementation would never
have counted a leech kill via this path at all**, the single most common one. (Leech's own
`TerrainImpact` override was also checked directly, since throw-impact was already a suspected
gap generally — confirmed it only ever calls `Stun(...)`, never damage or `Die()`, so leeches
specifically can't be killed by throwing them into a wall; that generic concern doesn't apply to
this species even though the underlying gap-pattern, "a creature-specific death path with no
`SetKillTag` call anywhere in it," is exactly the same shape.)

**Fix:** rather than special-case Leech's `airDrown` mechanism specifically, added a general
fallback source of attribution — `CreatureKillListHandlerHook` (Postfix on
`PhysicalObject.Grabbed(Creature.Grasp)`, the base method; checked and confirmed only `Fly.cs`/
`LanternMouse.cs` of this gimmick's tracked roster override it, and both call `base.Grabbed`, so
this gets real coverage) records "which tracked player most recently grabbed this creature" into a
new `KillListTracker._handlers` table (`ConditionalWeakTable<Creature, HandlerRecord>`, keyed on
the creature instance). `CreatureKillListGimmick`'s own `Die()` Postfix now only reaches for this
fallback when `killTag` is **entirely absent** (`killer == null`) — deliberately never overriding a
`killTag` that's already set to something else (e.g. eaten by another creature), since that's real,
known information that shouldn't be second-guessed by a weaker "was recently touched" heuristic.
The fallback's own window (`HandlerAttributionWindowTicks`, 2400 ticks / 60 seconds) is
deliberately much longer than `killTag`'s own ~5-second decay — sized against Leech's own
worst-case ~22.5-second `airDrown` timer with margin for the delay between being dropped and
actually drying out, while still short enough that "you touched this several minutes ago" can't
retroactively claim an unrelated later death. **This is the one death-path gap actually found and
fixed this round** — high confidence it correctly resolves the leech case specifically, since the
exact mechanism (`airDrown`) and its timing were read directly rather than inferred.

**Two more gaps found but deliberately NOT fixed yet, still open:** (1) killing something by
**throwing it into a wall/floor** for species where the creature-specific override actually applies
lethal impact damage (unlike Leech's own `TerrainImpact`, which never does) — `Creature.
TerrainImpact`'s *base* implementation only ever spawns cosmetic water-drip particles, never calling
`Violence()`/`Die()` itself, so wherever this really happens it must be creature-specific, and
checking all 55 individually was judged out of scope for this pass; the new handler-fallback above
would actually cover this case too now, PROVIDED the creature was recently grabbed by the tracked
player before being thrown — worth re-testing specifically now that the fallback exists. (2) a
**slow, multi-second spear stab** — `Spear.cs`'s own per-tick stab damage calls
`Violence(base.firstChunk, ...)` with the *spear's own* chunk as source (not a `Creature`), so
neither `killTag`'s own auto-set-on-Violence path nor (for a creature that was never grabbed by the
player, just stabbed from a distance) the new handler-fallback would apply if the kill happens after
`killTag`'s own 5-second decay from the initial thrown-weapon collision. **Both flagged for an
explicit in-game check**, not fixed blind — see the checklist at the end of this section.

**Only banked on a successful cycle end — a deliberate change from the first pass.** Originally
this wrote straight to `DeathPersistentSaveData` the instant a kill landed. Requested to change:
"if character dies, then it should not get counted... keep the memory of what got killed as you
are now, but only save it on successful cycle ending." `KillListTracker` now has two tiers: an
in-memory-only "pending" set per character (`RecordPendingKill`, called from the `Die()` Postfix,
same as before except it no longer touches the save file directly), and the real permanent record.
Two new Harmony patches decide which way a cycle's pending kills go:
- `CreatureKillListCycleWinHook` — Postfix on `RainWorldGame.Win(bool malnourished, bool
  fromWarpPoint)`. Confirmed via `grep -rn "\.Win(" reference/GameSource/*.cs` that this method has
  **exactly two callers, both in `ShelterDoor.cs`**, both firing at the exact moment a character
  successfully reaches and closes a shelter door — i.e. this genuinely only ever means "the cycle
  just ended in success," not some other, looser notion of winning. Merges every assigned,
  currently-in-session character's pending set into the real save data (`CommitPending`), then
  clears it.
- `CreatureKillListCycleGameOverHook` — Postfix on `RainWorldGame.GameOver(Creature.Grasp
  dependentOnGrasp)`. Confirmed via the same technique that all **4 callers, all in `Player.cs`**,
  are genuine character deaths (`PermaDie()`, a predator's den-drag "danger grasp" timing out at
  exactly 60 ticks, or an ordinary `Die()` immediately followed by `GameOver(null)` twice more).
  Discards (`DiscardPending`) rather than commits — a cycle ending in death loses that cycle's kills
  entirely, per the request. Deliberately does not try to second-guess Jolly Co-op's own more
  elaborate `JollyGameOverEvaluator` logic (whether GameOver "really" means everyone's dead in a
  multiplayer session) — this mod has no other Jolly-specific handling anywhere, and worst case a
  spurious call just clears an already-empty pending set (`DiscardPending` on a character with
  nothing pending is a safe no-op).

Persistence within `CommitPending` still follows `OneSpearGimmick.cs`'s own established marker
pattern: `DeathPersistentSaveData.unrecognizedSaveStrings` gets one marker per (character,
creature-type) pair the first time it's actually banked — `"PRIMAL_KILLLIST<dpB>" + characterID +
"<dpB>" + creatureTypeID`. Unlike OneSpear's indexed per-use markers (tracking a repeatable count),
this only ever needs one marker per pair, so the private `RecordKill` helper is simply idempotent.

**Progress display moved out of the Remix menu entirely — a second deliberate change.** The first
pass showed a live checklist in the gimmick's own Remix settings sub-view, which surfaced a real
limitation (a save slot's `PlayerProgression` only ever keeps one character's `SaveState` loaded at
a time, so the Remix character grid couldn't reliably show every character's progress). Requested
instead: "dont show it in the settings — put a button in the cycle ending screen that opens up a
window that lists all of the creatures killed — at the top list creatures that havent been killed,
then a separator, then the ones that have been killed." This actually *resolves* the earlier
limitation for free: the cycle-end screen (`Menu.SleepAndDeathScreen`) already has direct,
unambiguous access to the correct character's own `SaveState` via `KarmaLadderScreen.saveState` (no
guessing about which character is "currently mounted" needed at all). `GimmickDef.BuildSettingsUI`
is now left `null` for this gimmick — opening its Remix settings sub-view just shows the ordinary
"no additional settings" message every other settings-less gimmick shows.

Implementation, two new Harmony patches on `Menu.SleepAndDeathScreen` (the same screen
`NoPassagesGimmick.cs` already patches, reusing its exact `saveState`-reflection approach):
`KillListButtonHook` (Postfix on `AddSubObjects`, which runs once per screen right after vanilla's
own Continue/Exit/Passage buttons already exist) adds a "KILL LIST" button, positioned by
recomputing the same X/Y formula vanilla's own `exitButton` uses (`ContinueAndExitButtonsXPos`,
`SafeScreenOffset` — both plain public, already relied on by the very method being patched) rather
than reading `exitButton.pos` back, so it needs no additional new member access. `KillListSignalHook`
(Postfix on `Singal(MenuObject sender, string message)`) reacts to the click the same way every
other vanilla button on this screen works — `SleepAndDeathScreen.Singal` itself starts with
`base.Singal(sender, message)` then branches per-message for its own extra cases (confirmed via
decompilation), so a Postfix here runs after all of that regardless of which branch fired and can't
interfere with any real signal.

The popup itself is a plain `Menu.DialogNotify` (shown via `ProcessManager.ShowDialog`, the same
mechanism vanilla uses everywhere from `OptionsMenu` to `ExpeditionMenu` for "show some text, OK to
dismiss"), sized via `Menu.DialogBoxNotify.CalculateDialogBoxSize` — which grows to fit whatever
text it's given with **no built-in cap or scrolling**. A custom scrollable in-game `Page` was
considered and rejected for this pass: unlike a broken Remix tab (contained to the mod's own menu),
a broken cycle-end screen could block ordinary play entirely, and this container has no way to test
a hand-built Page-based UI before shipping it. Names are comma-joined into two paragraphs (not one
line per creature) specifically so `DialogNotify`'s own word-wrap keeps the box more reasonably
proportioned than ~55 forced line breaks would — but with up to 55 names across two sections, the
box can still plausibly run taller than the visible screen. Flagged as a known, unverified layout
risk rather than a solved problem.

**New reflection surface (Gotcha #1) this round:** nothing beyond what the first pass already
used (`Creature.killTag`, plus `KarmaLadderScreen.saveState` which `NoPassagesGimmick.cs` had
already established). `RainWorldGame.Players`/`AbstractCreature.state`/`PlayerState.
slugcatCharacter`/`PlayerState.dead` and `KarmaLadderScreen.ContinueAndExitButtonsXPos`/
`Options.SafeScreenOffset` are all trusted directly, judged the same low-risk "plain, ubiquitous,
obviously-meant-to-be-public data property" category as `Creature.dead`/`.creatureTemplate.type`
already are elsewhere in this codebase — not the narrow, easy-to-get-wrong utility-method category
`ConfigHolder.Save()`/`PlayerProgression.currentSaveState` turned out to be (the latter is no
longer used at all now that the settings-view approach is gone).

**Not built or tested in any way** — this container has no `.NET` toolchain (see "Container
environment notes" below), so this is written entirely from decompiled ground truth, not compiled
or run once. No default assignment and no `HomeCharacter` (Remix-menu color tag) — not requested
for any specific character, same as `NoPassages`. When this next reaches a real build+test cycle,
check, in order: (1) it compiles; (2) assign Kill List to a character, kill one of the tracked
species **by grabbing and eating it** on the Primal slot, and confirm
`BepInEx/LogOutput.log` shows nothing alarming (no `[CreatureKillList]` log line exists anymore --
this round removed that log statement along with the old immediate-persist path; check via the
dialog instead, next step); (3) reach a shelter successfully, then open the "KILL LIST" button on
the sleep screen and confirm that one species now shows under "KILLED"; (4) start a fresh cycle,
kill another tracked species, then deliberately **die** before reaching a shelter — open "KILL
LIST" on the death screen and confirm that cycle's kill was NOT added (still only the
previously-banked one shows); (5) fully quit and relaunch, reload the same character, reach a
shelter, and confirm the previously-banked kill survived the restart; (6) **specifically test a
leech kill** — pull a leech off, throw it somewhere dry, wait for it to die of `airDrown`
(~22.5 seconds), and confirm it's still correctly attributed via the new handler-fallback (this is
the gap actually found and fixed this round — highest-priority thing to confirm); (7) test a kill
via throwing a creature into a wall/floor for some OTHER species (no biting, no spear, no
explosion) and confirm whether it's attributed correctly now that the handler-fallback exists — a
negative result here (for a creature that WAS recently grabbed by the player before being thrown)
would mean the fallback's window or hook coverage needs another look; (8) similarly test a **slow
spear-stab kill** (hold a lizard down and stab repeatedly for several seconds rather than a single
quick throw, without ever grabbing it directly) to check whether the ~5-second killTag decay window
causes a miss the handler-fallback can't cover either (since the player never grabbed the lizard,
only stabbed it); (9) kill something NOT on the 55-species list (e.g. a Watcher creature) and
confirm nothing is recorded and nothing throws; (10) confirm the "KILL LIST" button only appears
for a character that actually has the gimmick assigned, and doesn't appear (or doesn't break
anything) for one that doesn't.

**Update (2026-09-15, later session, first one with a real game window on the user's own machine):
the popup itself was confirmed broken, in exactly the way flagged above as an unverified risk, and
rewritten.** The user hit it for real and sent a screenshot: the comma-joined text ran off both edges
of the screen, and there was no visible way to close it. Root-caused both from the decompiled
`Menu.DialogNotify`/`Menu.DialogBoxNotify` source rather than guessed: (1) `CalculateDialogBoxSize`
wraps text at a fixed internal width with no regard for the actual screen size at all -- a long
enough list was always going to overflow; (2) the specific constructor overload the old code called,
`DialogNotify(string, Vector2, ProcessManager)`, is the ONE overload that never calls `Init(Action
onOK)` -- the only thing that actually builds the OK button -- so this popup never had a close button
to begin with, confirming the user's own suspicion exactly.

Replaced with `KillListDialog` (new class, in `CreatureKillListGimmick.cs` right after
`KillListSignalHook`) per explicit request: "6 columns of an icon followed by a name," plus a
guaranteed-working close button. Built as a `Menu.Dialog` subclass (same base
`ShowDialog`/`StopSideProcess` mechanism, same free full-screen dark backdrop + centered panel +
Continue/Exit-blocking) rather than a custom `Page`, so only the CONTENT needed rebuilding. Icons
reuse vanilla's own "Kill_&lt;creature&gt;" sprite set via `CreatureSymbol.SpriteNameOfCreature`/
`ColorOfCreature`, given an `IconSymbol.IconSymbolData` -- the exact lookup vanilla's own
`KillsDisplayer` (`reference/GameSource/Menu/KillsDisplayer.cs`) already uses for a similar icon+
label row elsewhere, so no new art was needed. `KillListEntry` now also keeps the original
`CreatureTemplate.Type` object (not just its string ID) specifically so this lookup needs no
string-to-ExtEnum round-trip. Not-yet-killed entries render dimmed (vanilla's own MediumGrey);
killed entries "reveal" their real color. Size is computed once up front (`ComputeSize`, static,
since it has to run before the base constructor -- and `this` -- exists) from the same row-count
formula (`SectionRowsHeight`) the layout code itself uses afterward, so the two can't drift apart.
At the worst case (55 entries in one bucket) this comes to roughly 1300x650, comfortably inside the
1366x768 virtual canvas every `Menu.Dialog` centers against (confirmed in `Dialog.Init` -- those two
numbers are hardcoded there regardless of the player's real screen resolution) -- no scrolling needed
for this pass. See `KillListDialog`'s own doc comment in the source for the full account.

**Update (2026-09-15, same later session): the user tested it directly and reported it "working
relatively well," with two further fixes requested.**

1. **Alignment: section headers should be centered like the CLOSE button, and each entry's name
   text should sit right next to its icon instead of floating off to the right.** Root cause, found
   by reading `Menu.MenuLabel.GrafUpdate` (`reference/GameSource/Menu/MenuLabel.cs`): a `MenuLabel`
   always places its underlying `FLabel` at the exact CENTER of its own `pos`/`size` rect
   (`pos + size / 2`), regardless of `alignment` -- `alignment` only controls which direction the
   text extends FROM that fixed anchor point (Left = rightward, Center = symmetric, Right =
   leftward), not where the anchor itself sits. Both the section headers and the per-entry name
   labels had been given `FLabelAlignment.Left` on a box spanning the FULL available width, which
   anchors the text at that box's own horizontal middle and grows it rightward from there --
   nowhere near true left-alignment, and nowhere near true centering either. Fixed two different
   ways depending on what was actually wanted: the section headers just needed the explicit `Left`
   removed entirely, falling back to the constructor's own default (`Center`), which correctly
   centers the text within the box's real width, matching the CLOSE button's own look. The per-entry
   name labels needed the opposite: kept `Left`, but changed their box to a **zero-width** rect
   (`size.x = 0`) so the anchor collapses onto `pos.x` itself -- with zero width added, `pos + size/2
   == pos`, so `Left`-aligned text now genuinely starts exactly at `pos.x` (right after the icon)
   instead of at the middle of a wide column that was never actually needed for anything else.
2. **Kill List progress didn't survive a game restart.** Root cause, found by reading
   `RainWorldGame.Win`'s decompiled body all the way through rather than assumed: `Win` calls
   `SaveState.SessionEnded(..., survived: true, ...)`, and `SessionEnded`'s own success branch
   (`reference/GameSource/SaveState.cs`, ~line 473) calls `progression
   .SaveWorldStateAndProgression(malnourished)` -- the actual save-to-disk call -- SYNCHRONOUSLY,
   before `Win` itself ever returns. `CreatureKillListCycleWinHook` was a **Postfix** on `Win`,
   which only ever runs after the entire original method (including that nested disk write) has
   already finished -- so every kill got committed to `unrecognizedSaveStrings` one save too late.
   It was real in memory for the rest of that session (re-opening the popup later the same session
   showed it correctly, which is exactly why this wasn't caught until a real restart), but never
   reached the disk write `Win` itself had already triggered, so a save that was never touched again
   before quitting lost it outright. Exactly the same "Postfix runs one step too late" shape as
   TimeLimitGimmick's and Slugpup's own earlier timing bugs (see this doc's other Gotchas) -- just
   newly found in this gimmick. **Fixed by changing `Postfix` to `Prefix`** -- everything the method
   reads (`Players`, `IsStorySession`, `GetStorySession`/`saveState`) already exists for the whole
   cycle rather than being constructed fresh by `Win`, so running earlier changes nothing about
   correctness, only timing relative to the disk write. `CreatureKillListCycleGameOverHook` (the
   discard-on-death path) did NOT need the same fix -- it only ever touches the in-memory `_pending`
   dictionary, never `unrecognizedSaveStrings`, so save-write timing was never relevant there.

Both fixes built clean and deployed. **Not yet re-confirmed in-game as of this doc** -- next, in
order: (1) headers visually centered, entry names sitting flush against their icons; (2) kill a
tracked species, reach a shelter, fully quit and relaunch the game, reopen the Kill List popup for
that character, and confirm the kill is still there (the specific case that was broken).

## Primal character names, in detail

Requested by the user right after deprioritizing the config-menu UI merge above (2026-09-11):
while on the alt slot, Gourmand and Spearmaster should show up on the "choose your character"
screen as **"Primal Gourmand"**/**"Primal Spearmaster"** (shown all-caps, matching vanilla's own
"THE GOURMAND"-style convention: "PRIMAL GOURMAND"/"PRIMAL SPEARMASTER"), with a description that
reflects whichever gimmicks are currently assigned to them — not a fixed, hardcoded blurb.

**Where this lives on screen:** Rain World only shows a character name + description at all on
`Menu.SlugcatSelectMenu.SlugcatPageNewGame` — the page shown for a character that does *not yet*
have any save data on the current slot ("new expedition"... no, this is story mode: it's the
ordinary "pick a slugcat to start playing" page). Once that character has actual progress on this
slot, the game switches to `SlugcatPageContinue` instead, which shows cycle/karma/time stats and
has **no** name/description labels at all — so this feature is only ever visible before a run has
been started with that character on the alt slot. That matches what the user asked for (the
"choose your character" screen) and there was nothing else to hook for an already-started save.

**Implementation** (`Gimmicks/PrimalCharacterNames.cs`): a Postfix Harmony patch on
`SlugcatPageNewGame`'s constructor. Vanilla's own constructor (see
`reference/GameSource/Menu/SlugcatSelectMenu.cs`) sets two fields, `difficultyLabel` (the name) and
`infoLabel` (the description), based on which vanilla character is being shown — this patch runs
right after that and, only if `PrimalSlot.IsActive(menu.manager.rainWorld)` and the character is
Gourmand, Spearmaster, Sofanthiel, or Artificer, overwrites both with the Primal name and a
gimmick-driven description; any other character (or any of these four off the alt slot) is left
completely untouched.

**Read via reflection, not direct field access** — see the 2026-09-11 addendum to **Gotcha #1**
above for why: `reference/GameSource/` is decompiled from the same publicized assembly as
`lib/PUBLIC-Assembly-CSharp.dll`, so its `public` markings on `difficultyLabel`/`infoLabel` prove
nothing about the real assembly's accessibility, and this session's container had no way to
actually test whether a direct `__instance.difficultyLabel` access would throw
`FieldAccessException` the way `PlayerGraphics.player` once did. `AccessTools.Field(...).GetValue`
sidesteps the question entirely. Setting `.text` on the `MenuLabel` reference once retrieved is
ordinary code (not reflection) — `MenuLabel.text` is a normal public property used identically
across dozens of unrelated vanilla menu classes, not a narrowly-scoped simulation field, so it's a
much safer bet, and there's already a proven-safe precedent for the `SlugcatStats.Name ==
MoreSlugcatsEnums.SlugcatStatsName.X` comparison this patch also uses (see
`OneSpearGimmick.cs`'s `player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear`, already
compiling and running in this same codebase).

**The description text** is intro flavor line + `"Active traits: "` followed by the display names
of every `GimmickRegistry` entry currently `GimmickAssignments.IsEnabled` for that character (or a
placeholder line if none are assigned yet) — built fresh every time the page is constructed, so it
always reflects live Remix state rather than a hardcoded snapshot of today's defaults. (As of
2026-09-15, this only applies to the four characters with real written content -- see below for the
other six, which get a static description instead.) If a gimmick is ever added for a character not
already covered by this switch, extend the `if`/`else if` chain rather than assuming a new
character automatically deserves a written intro line.

**This is the file to edit for a character's alt-slot display name or description.** Each
`if (slugcatNumber == ...)` branch above sets a literal `name` string (shown all-caps on screen,
matching vanilla's own "THE GOURMAND"-style convention) and an `introLine` string passed into
`BuildDescription` (the "Active traits: ..." line is appended automatically — don't include it
yourself). To change what's shown for an existing character, edit that branch's two string
literals directly; to add a new character, copy one of the existing `else if` blocks and change the
`SlugcatStatsName` comparison, the `name`, and the `introLine`. This only ever affects the alt slot
(`PrimalSlot.ReservedSlot`) and only this one screen (`SlugcatPageNewGame`) — see **"Config menu UI, in
detail"** below for the *other* place a character's name shows up (this mod's own Remix settings
screen, a separate and unrelated label).

**2026-09-12: Artificer added.** Once Artificer got its own default gimmick (Chains — see
"Chains gimmick, in detail"), the user asked for it to follow the same "Primal" treatment as
Gourmand/Spearmaster: added an `else if` branch for `MoreSlugcatsEnums.SlugcatStatsName.Artificer`
→ **"PRIMAL ARTIFICER"**, with the same generic intro sentence style as the other two ("This
Artificer has been changed by something primal..."). The user later hand-edited this intro line
directly on their own machine (see the 2026-09-13-ish addendum on the `ChainsGimmick.cs` push,
above) to a vanilla-flavored line instead — that hand edit was deliberately left in place.

**2026-09-15: intro lines rewritten by the user; Sofanthiel added.** The user asked to see every
Primal character's current name/description, then sent back rewritten intro lines for Gourmand and
Spearmaster and a brand-new one for Sofanthiel (vanilla's "???"). Applied verbatim:
- **PRIMAL GOURMAND** — "A glutton reduced to the simplest pleasures of life. Capable only of
  sluggish rolling, one must seek out their home to regain some semblance of power."
- **PRIMAL SPEARMASTER** — "A deteriorating abnormality with its crucial needles running out. A
  traveller from a very far away land; A feeling in your hollow depths rushes you, messenger..."
- **PRIMAL ???** (new `else if` branch, `MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel`) — "Thanks
  Rockman." The label follows vanilla's own convention of naming this character "???" rather than
  "THE X" (see `reference/GameSource/Menu/SlugcatSelectMenu.cs`'s Sofanthiel branch), so it becomes
  "PRIMAL ???" rather than a literal "PRIMAL SOFANTHIEL". Unlike Gourmand/Spearmaster/Artificer,
  Sofanthiel has no gimmick assigned (none exists for it), so its "Active traits" line will always
  read the generic "No primal traits assigned yet" placeholder — this is a standalone flavor-text
  addition, not tied to a gimmick the way the other three are.
Artificer's description was **not** touched this round (the user's rewrite list didn't include it;
it keeps the hand-edited vanilla-flavored line from 2026-09-12/13 above). Same container limitation
as every other entry in this file — no `.NET` toolchain here, so none of this has compiled or run;
the only change here is to string literals and one new `else if` branch mirroring three already-
proven branches, so the risk is low, but still unconfirmed in-game.

**Build/deploy status:** this feature has gone through several successful builds since it was first
written (the Chains-gimmick and settings-import passes both compiled `PrimalCharacterNames.cs`
along with everything else without incident), so the toolchain/compile-time risk flagged when this
section was first written no longer applies — what's actually unconfirmed now is purely the
in-game behavior: does `AccessTools.Field` actually find `difficultyLabel`/`infoLabel` at runtime,
and does the screen show the expected text. See Outstanding items for the exact checklist.

**2026-09-15, later the same day: the remaining six characters given a "Primal" label too.**
Requested plainly: "for all characters that arent the 4 I have written descriptions for (inv,
artificer, gourmand, spearmaster) rename them to Primal (their normal name) followed by a
description that is 'Primal version not implemented'." Added six more `else if` branches --
Survivor (`SlugcatStats.Name.White`) → **PRIMAL SURVIVOR**, Monk (`Yellow`) → **PRIMAL MONK**,
Hunter (`Red`) → **PRIMAL HUNTER**, Rivulet → **PRIMAL RIVULET**, Saint → **PRIMAL SAINT**, and
Watcher (guarded by `ModManager.Watcher`, since it's a separate DLC from MSC) → **PRIMAL WATCHER**
-- each set to a single new `const string NotImplementedDescription = "Primal version not
implemented";`, deliberately **not** routed through `BuildDescription` (no "Active traits" line
appended): these six have no gimmicks of their own to report, and the request asked for that exact
literal text, not a dynamic one. The old catch-all `else { return; }` branch (which used to leave
every character but the original four completely vanilla) now only applies to a hypothetical future
DLC character this mod hasn't accounted for at all -- every currently-known character is handled by
one of the ten branches above it. Same container limitation as every other entry in this file (no
`.NET` toolchain here) -- the change is six mechanically similar string-literal branches mirroring
already-proven ones, so risk is low, but still unconfirmed in-game.

## Character-select portrait art, in detail (relevant if an artist edits these)

**2026-09-11:** the user wants an artist to edit the character-select portraits (starting with
Spearmaster, likely toward custom art for Primal Gourmand/Spearmaster eventually) and first tried
editing `C:\Program Files (x86)\Steam\steamapps\common\Rain World\RainWorld_Data\StreamingAssets\
mods\moreslugcats\scenes\slugcat - spearmaster\slugcat - spearmaster - flat.png` directly. The edit
saved to disk (confirmed via `device_list_dir` — file grew from 233,255 to 274,577 bytes with a
fresh mtime) but had **no visible effect in-game**.

**Root cause:** the character-select screen doesn't render a single flat image. Per the decompiled
`reference/GameSource/Menu/SlugcatSelectMenu.cs`, the portrait is built as
`new InteractiveMenuScene(menu, this, sceneID)` — a live, multi-layer composite — and whether that
layered scene or a pre-flattened single image is used is gated by a `RainWorld.flatIllustrations`
flag (e.g. `if (!manager.rainWorld.flatIllustrations && ...)` at several points in that file). This
flag is a lower-detail/performance fallback, off under normal graphics settings. `MenuScene.cs`/
`InteractiveMenuScene.cs` themselves aren't in this project's decompiled `reference/GameSource/`
subset, so the exact file-loading code wasn't directly readable — but the scene folder's own
contents confirm the mechanism:

```
positions.txt                              -> 4 lines: "284, 150" / "425, 132" / "370, 340" / "450, 140"
spearmaster slugcat - 1.png                -> layer 1: the actual character portrait
spearmaster slugcat - 1 - dark.png         -> layer 1, ascended/dead variant
spearmaster vines - 2.png                  -> layer 2
spearmaster pipes - 3.png                  -> layer 3
spearmaster background - 4.png             -> layer 4
slugcat - spearmaster - flat.png           -> separate pre-flattened composite (flatIllustrations only)
slugcat - spearmaster dark - flat.png      -> pre-flattened, ascended variant (flatIllustrations only)
slugcat - spearmaster - flat.png~          -> stray editor backup file, harmless
slugcat - spearmaster - flat (2).png       -> stray duplicate, harmless
```

`positions.txt` has exactly 4 coordinate pairs, matching layers 1–4 by their filename suffix — the
game composites those four PNGs live, at those positions, for the on-screen portrait under normal
settings. The two "flat" files are a fallback the select screen doesn't read unless
`flatIllustrations` is forced on, which is why editing `flat.png` produced no visible change.

**For the artist:** edit `spearmaster slugcat - 1.png` (the actual portrait layer) to prove/see
changes — this is what's rendered by default. If the scene's background/vines/pipes are also in
scope, those are the numbered layer files above; don't touch `positions.txt` unless a layer's
on-screen position itself needs to change. Every other MSC character has the same per-character
folder structure under `mods\moreslugcats\scenes\slugcat - <name>\` (vanilla Survivor/Monk/Hunter
use `StreamingAssets\scenes\slugcat - <color>\` directly, one level up, without the `mods\
moreslugcats\` prefix — confirmed by listing that folder, which only has `slugcat - red/white/
yellow`). **Not yet verified against a live edit-and-relaunch test** — this is a source-level
explanation, not something this session watched render; if editing the layer-1 portrait still
doesn't show up, check the game's graphics quality option (`Options.Quality`, referenced alongside
`flatIllustrations` in the same file) in case this particular install has it forced to a low/flat
setting.

## Primal character portraits, in detail (blacking out the six "not implemented" characters)

**The request:** immediately after asking for the six "not implemented" name/description branches
above, in the same message: "look into how the character art seen in the selection menu is loaded
and filter it to be completely black - this should mean you need to black out the '-1' image in
the characters associated image folder." Implemented in the new `PrimalUnimplementedPortraits.cs`.

**How the portrait is actually loaded, traced properly this time** (the section above, "Character-
select portrait art, in detail," already covers the high-level mechanism -- multi-layer
`MenuDepthIllustration`s per character, not a single flat image -- but this round needed the exact
construction code, which is now available in `reference/GameSource/Menu/MenuScene.cs` where it
wasn't before). `MenuScene.BuildMSCScene`/`BuildScene` build each character's select-screen scene
via a dedicated method (`BuildSpearScene`, `BuildGourmandScene`, the base-game
`Slugcat_White`/`Slugcat_Yellow`/`Slugcat_Red` branches, etc.), each calling
`AddIllustration(new MenuDepthIllustration(menu, this, sceneFolder, "<fileName>", pos, depth,
shader))` once per layer. For Spearmaster: `"Spearmaster Background - 4"`,
`"Spearmaster Pipes - 3"`, `"Spearmaster Vines - 2"`, then `"Spearmaster Slugcat - 1"` (or
`"...- 1 - Dark"` if locked) added last, on top -- that last one is the actual character portrait.

**The "-1" shorthand doesn't generalize, but a different pattern does.** Checking every
character's own scene-builder found the numeric suffix on the portrait layer varies -- Spearmaster
and Monk (`"Yellow Slugcat - 1"`) are both "- 1", but Survivor's is `"White Slugcat - 2"` and
Hunter's is `"Red Slugcat - 1"` again, with no consistent rule. What *is* consistent: every
decoration/background layer is named after what it depicts (Background, Vines, Pipes, Haze,
Specks, BkgPlants, FgPlants, Spears...), and only the actual character-portrait layer's filename
contains the literal word "Slugcat" -- confirmed across every character's builder method in
`MenuScene.cs`. So the implementation matches on `fileName.Contains("Slugcat")` rather than parsing
out a number, which correctly generalizes across every character checked rather than special-
casing Spearmaster's own "- 1" as if it were universal (a mistake in the same spirit as the
`standardVoidSea` naming trap from the Slugpup ending work earlier the same day -- a shorthand the
user reached for to describe one concrete example isn't necessarily the general rule, so it's
worth checking against more than one character before committing to a pattern).

**Why a Postfix on `GrafUpdate`, not a one-time color set.** `MenuIllustration` (the base class)
exposes a plain `Color color` property that wraps `sprite.color` — trivial to set to black. The
complication: `MenuDepthIllustration.GrafUpdate` (the override actually used at render time)
recomputes `sprite.color` itself every frame from the illustration's size/depth (feeding a custom
blur shader, unrelated to literal display color) -- but only `if (shader != MenuShader.Basic)`.
Every MSC character's own portrait layer is built with `MenuShader.Basic` (see `BuildSpearScene`
above), so for those a one-time color set would have actually been fine. But the three base-game
portraits (`"White Slugcat - 2"`, `"Yellow Slugcat - 1"`, `"Red Slugcat - 1"`) use
`ModManager.MMF ? MenuShader.Basic : MenuShader.LightEdges` -- on any install without the MMF mod,
that's `LightEdges`, and the per-frame recompute *would* silently overwrite a one-time black back
to its own encoded value, the exact same "quiet, no visible effect" failure mode this project has
hit before (Gotcha #6-style). Postfixing `GrafUpdate` itself and re-applying black *after* the base
method runs sidesteps the question of which shader a given character happens to use entirely --
correct either way, with no need to special-case shader type or guess whether MMF is installed.

**Scope and guards:** only fires while `PrimalSlot.IsActive`, and only for the illustration whose
`owner` (a `MenuScene`) has one of the six target characters' own `SceneID`s
(`MenuScene.SceneID.Slugcat_White/Yellow/Red`; `MoreSlugcatsEnums.MenuSceneID
.Slugcat_Rivulet(_Cell)/Slugcat_Saint(_Max)`, guarded by `ModManager.MSC`; `Watcher.WatcherEnums
.MenuSceneID.Slugcat_Watcher`, guarded by `ModManager.Watcher`). Since this hooks the illustration's
own render call rather than a specific screen, it applies equally to the "choose your character"
(new-game) screen and the "continue" screen for an existing save on one of these characters, with
no extra work -- both reuse the same scene-building code and the same portrait assets.

**Deliberately not handled:** `flatMode`/`flatIllustrations` (the pre-flattened single-image
fallback mentioned in "Character-select portrait art, in detail" above) -- confirmed in that
earlier investigation to be off under normal graphics settings, so a second code path just for that
rare case was judged not worth adding without a concrete reason to think it's reachable in practice.

**Not yet built or tested — same container limitation as everything else in this file (no `.NET`
toolchain here).** Every new member access (`MenuDepthIllustration`/`MenuIllustration`'s
`fileName`/`sprite`/`color`, `MenuObject.owner`, `MenuScene.sceneID`/`.menu`) was checked against
`reference/GameSource/Menu/MenuScene.cs`, `MenuIllustration.cs`, and `MenuDepthIllustration.cs`
rather than guessed -- all plain `public` members on ordinary, widely-reused vanilla menu classes,
consistent with the same judgment call already made for `SlugcatPageNewGame`'s labels (Gotcha #1).
Next, in-game, in order: (1) it compiles; (2) on the alt slot, each of the six "not implemented"
characters' portrait renders as a solid black silhouette (shape preserved via alpha, color
zeroed), with the surrounding background/vines/pipes/plants layers still visible and unmodified;
(3) Gourmand/Spearmaster/Artificer/Sofanthiel's own portraits are completely untouched; (4) off the
alt slot, every character's portrait (including these six) renders completely normally; (5) if any
of the three base-game portraits (Survivor/Monk/Hunter) still show their real color, check whether
MMF is installed and confirm the `LightEdges`-shader per-frame overwrite theory above by testing
whether the *other* two (MSC-only) base-game-adjacent characters (Rivulet/Saint, both `Basic`-
shaded) blacked out correctly while these three didn't -- that would confirm the shader-dependent
timing issue precisely, the same way this project has isolated similar "some cases work, some
don't" bugs before.

## Slugpup gimmick stats rescaling, in detail (fix for "Primal Gourmand looks a little thin")

**This whole section is a record of a real struggle, not a clean one-shot fix — keep it that way
rather than tidying it into "and then it worked."** Getting Gourmand to actually look and behave
like a proportional pup (as opposed to a generic tiny Slugpup wearing Gourmand's name) took four
rounds, each one a plausible-looking fix that turned out to be incomplete or subtly wrong in a way
that produced no error message at all — "no change" and "wrong color" rather than a crash. In
order: (1) naive `SlugCatClass` flip alone — compiles, runs, no exception, silently wrong (too
thin); (2) rescale `npcCharacterStats` from a Postfix on `Player`'s constructor — compiles, runs,
no exception, still silently wrong (still thin, "no change") because it ran one line too late
within that same constructor; (3) same rescale, moved to a Postfix on the right constructor
(`NPCStats`'s) — logically correct, but a *second*, independent bug (wrong color) surfaced in the
same test pass, from a completely different vanilla code path than the one being fixed; (4) the
color fix compiled against a type name (`NPCStats`) that doesn't actually exist unqualified in the
real assembly, caught immediately by `dotnet build` (`CS0246`) rather than silently, because it's
a compile-time name-resolution error rather than a runtime accessibility one. The throughline: this
particular gimmick sits on top of several fragile, undocumented vanilla assumptions (a specific
sub-constructor's call order, a color method routing through a completely different property than
the one that looks intuitively right, a nested-type name), and every one of the wrong turns above
looked like a completely reasonable fix until tested. Future work here should expect more of the
same rather than assuming the current state is finally exhaustive — see the still-open question at
the end of this section.

**The bug report:** after the Slugpup gimmick's naive `Player.SlugCatClass =
MoreSlugcatsEnums.SlugcatStatsName.Slugpup` flip, Primal Gourmand "looks a little thin" in-game.
User's own diagnosis, which turned out to be exactly right: *"I believe this is because you didnt
fully copy the logic of pupify, which adds extra logic to downpour character slugpup versions."*

**Root cause, ground-truthed by disassembling the real game assembly.** No .NET decompiler
(ikdasm, ilspycmd, dotnet, mono, monodis) was installable in the container this was investigated
in — `pip3 install` failed for every package tried, decompiler or not, a genuine environment
restriction, not a task-specific one — so a small hand-rolled pure-Python ECMA-335 PE/metadata
parser and CIL disassembler was written from scratch instead (see "Container environment notes"
below for where that tool lives; it's reusable for any future reverse-engineering need on this
project). Used against both `PUBLIC-Assembly-CSharp.dll` (the game's own reference assembly, same
one this mod compiles against) and `Pupify.dll` (Steam Workshop item 3005429221, an installed
third-party mod the user pointed at as the reference implementation), it revealed:

- `Player.slugcatStats` (the getter almost everything reads — body mass, run speed, visibility,
  lung capacity, etc.) branches on `Player.isSlugpup`: if true, it returns `Player.npcCharacterStats`
  (a private, per-instance field) instead of the game's normal per-character stats.
- `Player.isSlugpup` is simply `ModManager.MSC && SlugCatClass == Slugpup` — so the gimmick's flip
  alone is enough to route reads through `npcCharacterStats`.
- `npcCharacterStats` is built exactly once, inside `Player`'s own constructor, right after
  `GetInitialSlugcatClass` runs: `if (isSlugpup) npcStats = new NPCStats(this);` — and `NPCStats`'s
  constructor, as a side effect of building itself, is what actually populates
  `npcCharacterStats`. Its own logic: seed `Random` from the creature's ID, roll five independent
  personality values (Bal/Met/Stealth/Size/Wideness, each `Random.Range(0,1)` raised to the 1.5
  power), build a **throwaway generic `new SlugcatStats(SlugcatStatsName.Slugpup, malnourished)`**
  — i.e. plain baseline Slugpup, with no reference to whatever character is actually being played
  — then multiply that baseline's fields by small personality-driven factors centered near 1.0
  (e.g. `bodyWeightFac *= (0.85 + 0.15*Wideness) + 0.10*Met`).
- In short: vanilla's `isSlugpup` path was built for genuine generic NPC slugpups (and Jolly
  Co-op's own "play as a nameless pup" mode), where there's no other identity to preserve. Once a
  real vanilla character gets routed down this same path, their real proportions (Gourmand's own,
  considerably-larger-than-generic-pup `bodyWeightFac` included) are simply discarded and replaced
  with a randomized near-1.0x variation of generic Slugpup's own — hence "looks a little thin".

**The fix, ported from Pupify.** Pupify exists specifically to let a Jolly Co-op companion player
keep their own character's build while playing as a pup. Disassembling its
`PlayerHooks.SlugcatStats_ctor` hook (a MonoMod IL hook wrapping `SlugcatStats`'s own constructor)
found the exact scaling it applies, relative to a reference `SlugcatStats` for the real underlying
character (Pupify's own literal float constants, kept exact — see `SlugpupStatsGimmick`'s doc
comment in `Gimmicks/SlugpupGimmick.cs` for the full list): `bodyWeightFac *= 0.6499999761581421f *
1.0256409645080566f` (~x0.667), plus seven more fields (`runspeedFac`, `generalVisibilityBonus`,
`visualStealthInSneakMode`, `loudnessFac`, `lungsFac`, `poleClimbSpeedFac`,
`corridorClimbSpeedFac`, `throwingSkill`) scaled or offset similarly. (Pupify's own hook is much
larger than just this formula — most of it is Jolly Co-op multiplayer player-slot bookkeeping,
irrelevant to Primal' single-character-per-slot design, and was not ported.)

**First implementation (wrong, corrected below):** a new patch, `SlugpupStatsGimmick`, Postfixing
`Player`'s constructor `(AbstractCreature, World)` — built a throwaway
`new SlugcatStats(playerState.slugcatCharacter, malnourished: false)` for the real identity, then
overwrote `npcCharacterStats`'s 8 fields using Pupify's formula (via `AccessTools` reflection,
since both `Player.npcCharacterStats` and all of `SlugcatStats`'s fields are non-public in the
real assembly — Gotcha #1). Pushed, built, copied to the game folder (DLL timestamp confirmed
newer than every source file, including this one), and tested in-game. **User report: "no
change."**

**Root cause of "no change", found by tracing Player's constructor IL further:** the two main
`BodyChunk`s are created a little further down in that *same* constructor, and their mass is
computed from `slugcatStats.bodyWeightFac` *right there at chunk-creation time* — i.e. from
whatever `npcCharacterStats` held at that exact moment, not whatever it holds once the whole
constructor finishes. A Postfix on the entire constructor necessarily runs too late: both
BodyChunks already had their wrong, generic-random mass permanently baked in by the time it fired,
and nothing afterwards re-reads `bodyWeightFac` to redo that (`Player.setPupStatus` does exactly
this re-derivation, but only when `PlayerState.isPup` is set — a separate flag this gimmick
doesn't touch). The rescaled field values themselves were correct; they just took effect one step
too late to matter, in exactly the same "quiet, no-exception, looks like nothing happened" way
Gotcha #6 already documented for a different method.

**The fix:** patch `NPCStats`'s own constructor instead — `NPCStats(Player player)`, the thing
that actually builds `npcCharacterStats` as a side effect of building itself. A Postfix on it
fires the instant that nested call returns, which is *before* `Player`'s constructor resumes on to
create the BodyChunks — closing the timing gap. This also needed one new guard the first attempt
didn't have: `NPCStats`'s constructor isn't exclusive to real players wearing this gimmick — it's
the same constructor genuine AI-controlled NPC slugpups go through too (`Player.isNPC == true`;
they're implemented as ordinary `Player` instances). Added an explicit `!self.isNPC` check so this
never touches an actual wandering/companion pup's stats.

**A second, separate bug was reported in the same message: "the gourmand pup i play as is the
wrong color."** Ground-truthed the same way, by disassembling `PlayerGraphics.ApplyPalette` (the
method that decides the player's base body-part color every frame):
`if (SlugCatClass == Slugpup) baseColor = player.ShortCutColor(); else baseColor =
PlayerGraphics.SlugcatColor(CharacterForColor)`. `CharacterForColor` (a separate property, not
reached at all once the `SlugCatClass` branch is taken) correctly reads
`playerState.slugcatCharacter`, the real identity, and `SlugcatColor(name)` is exactly vanilla's
own per-character palette (Gourmand's own tan/pink hardcoded as `(0.941, 0.757, 0.592)`, Rivulet's
own blue, etc.) — the path every *non*-pup-flagged character already uses correctly. Once
`SlugCatClass` flips, `ApplyPalette` stops calling `SlugcatColor` entirely and instead colors the
body from `Player.ShortCutColor()` — vanilla's per-creature shortcut/pipe-transition tint.
Disassembling that method found why it looks "wrong": for `isSlugpup == true` it has a few
hardcoded special-case colors keyed off the creature's own RNG seed (for specific named vanilla
pups), and otherwise falls back to converting `Player.npcStats`'s own H/S/L fields (a different
field from `npcCharacterStats` above, but populated by that same `NPCStats` constructor) to RGB —
and those H/S/L values are themselves random, rolled from the creature's ID the same way the
personality stats are. Two playthroughs of the same character can get two different "wrong"
colors, because it's tied to a per-spawn creature ID, not to anything about the real character.
**This fix is Primal' own, not ported from Pupify** — Pupify's own decompiled hooks never
touch color at all (grepped for every color/palette-related identifier across all 5 of its
decompiled hook methods — zero hits), so pup companions in that mod render with this same random
color; it's a real, independently-discovered gap, not something Pupify already solved. The fix
just uses vanilla's own existing `SlugcatColor(SlugcatStats.Name)` lookup directly (invoked via
reflection, same accessibility-uncertainty reasoning as everywhere else in this file), applied to
the real identity — no new color logic invented. Implemented as a Postfix on
`Player.ShortCutColor()` itself (rather than patching `ApplyPalette`, which has many other
unrelated branches), so every caller of it for this player — the body palette above, and actual
shortcut/pipe-transition tinting — sees the real character's color consistently.

Both fixes (`SlugpupStatsGimmick` now on `NPCStats`'s constructor, and the new
`SlugpupColorGimmick` on `Player.ShortCutColor()`) live in the same
`Gimmicks/SlugpupGimmick.cs`. Pushed to the device and verified landed (21,324 bytes, confirmed via
re-stage/byte-compare after one silent-write retry — see the maintenance-policy note on
`device_commit_files`'s silent-failure history, which recurred here too).

**Fourth round — a real compile error, the first one this whole investigation actually hit:** user
ran `dotnet build` and got `CS0246: The type or namespace name 'NPCStats' could not be found`,
pointing at the `[HarmonyPatch(typeof(NPCStats), ...)]` attribute on `SlugpupStatsGimmick`. Checked
the game assembly's own metadata (`NestedClass` table) rather than guessing: `NPCStats` is a type
nested inside `Player` (`TypeAttributes.NestedPublic`, enclosing class = `Player`), not a
top-level/global type like `Player`, `SlugcatStats`, or `PlayerGraphics` all are — so its real,
compilable name is `Player.NPCStats`. This is a different failure mode from every other
Gotcha-#1-style bug in this doc: those compile clean and throw at runtime because the *reference*
assembly is publicized (over-permissive) relative to the real one; this one is a plain C#
name-resolution error that the publicized reference assembly would have caught as fast as the real
one — nesting isn't something publicizing hides. Fixed by changing the attribute to
`typeof(Player.NPCStats)`; the doc comments in the file still say plain "NPCStats" for readability
since C# users would recognize it as an inner-type reference either way. Re-verified no other bare
`NPCStats` references existed in the file (only the one in the attribute needed qualifying — every
other mention was inside a doc comment or a variable/field name, not a type reference). Pushed and
verified landed (21,606 bytes).

**Status as of this update: pushed, and the one build attempted so far only got as far as this one
compile error before it was caught and fixed — the corrected version has NOT yet been built or
tested in-game at all.** Don't treat this as "done" until a build actually succeeds and the user
confirms in-game that Primal Gourmand (a) no longer looks thin as a pup and (b) renders in his own
tan/pink color, ideally cross-checked against the `[Slugpup] ... npcCharacterStats rescaled from
real stats (...)` log line. Given the track record above (four rounds, each with its own distinct
failure mode), don't be surprised if this needs a fifth — see "Outstanding items" below for exactly
what to check next.

## Slugpup gimmick: SlugCatClass-flip rewrite, in detail (fix for "not fat enough" / "looks like a slugpup")

**The report:** with the stats and color fixes above already in place and tested, the user reported
back: "he had the right color, but wasn't fat enough, for lack of a better word... currently primal
gourmand looks like a slugpup but he should look like a more fat version, like what pupify does."
This is a *third*, distinct bug from the two already fixed above — proof the "keep expecting more
of the same" warning at the top of the stats-rescaling section was well-founded.

**Root cause: the original architecture itself, not a missing rescale.** `SlugpupGimmick` flipped
`Player.SlugCatClass` to `SlugcatStatsName.Slugpup` outright. That successfully routes
`Player.slugcatStats` through the rescaled `npcCharacterStats` (fixing weight/speed/etc.), but
`SlugCatClass` is *also* the exact field `PlayerGraphics.DrawSprites` branches on directly to decide
body-fatness sprite scale — confirmed by reading `reference/GameSource/PlayerGraphics.cs` around its
head/body `scaleX` assignments: `if (SlugCatClass == Gourmand) scaleX = 1.4f + ...` (head) / `1.6f +
...` (body); `else if (SlugCatClass == Spear) scaleX = 0.76f + ...`; `else if (SlugCatClass ==
Slugpup) scaleX = 0.9f + 0.2f*Wideness + ...` (a much narrower 0.9–1.1 range, keyed off the random
personality stat, nothing to do with the real character at all). Once `SlugCatClass` becomes
`Slugpup`, the `Gourmand` branch (1.4×/1.6×, the actual source of Gourmand's signature fatness) can
never be reached again — the pup renders using the generic Slugpup formula instead, which is exactly
"looks like a slugpup" rather than "a fatter version of Gourmand." The same `SlugCatClass` field
also gates unrelated per-character abilities and behavior throughout the codebase (diet/food-value
lookups, and any other `SlugCatClass ==` check anywhere in the game), meaning the original
implementation was silently breaking *all* of those too for as long as a character was pup-ified —
directly contradicting the user's own original framing of this gimmick when first requested: "play
as a slugpup while also changing behavior so that you can use that character's abilities as a
slugpup." The original file-map bullet's claim that the flip "alone is confirmed working" was true
only in the narrow sense that the character *becomes pup-sized and pup-behaving* — it was never
actually checked against "and still looks/acts like the real character," because nobody had reason
to suspect the flip cost anything until this specific report.

**The fix: re-read Pupify's own actual mechanism instead of re-deriving the trick from scratch.**
This container has a committed, already-decompiled copy of Pupify at
`reference/ExampleMods/Pupify/Pupify.decompiled.cs` (3,545 lines — no live disassembly needed this
round). Grepping it specifically for `PlayerGraphics` and `isSlugpup` (a question the original
stats-rescaling investigation never asked, since it was only looking for the stats-constructor hook
at the time) found `PlayerHooks.Player_isSlugpup` (~line 2679): a hook on `Player.isSlugpup`'s
*getter itself* — `if (!__instance.isNPC && !Plugin.MakeChanges(__instance)) return true; __result =
true; return false;` — that forces the getter to `true` for a pup-ified player *without ever writing
to `SlugCatClass`*. Cross-checked against `PlayerHooks.Player_GetInitialSlugcatClass` (~line 1982),
which explicitly guards `!...Room.world.game.IsStorySession` and otherwise just calls
`orig.Invoke(self)` (vanilla's own unmodified logic) — confirming Pupify deliberately leaves
`SlugCatClass` alone in ordinary single-player story sessions and gets "is a pup" entirely from the
`isSlugpup` override instead. This is the actual reason a Pupify companion pup looks like a smaller
version of their own real character rather than a generic slugpup: every `SlugCatClass ==`-gated
system (fatness scale, diet, abilities) keeps seeing the real identity, while every `isSlugpup`-gated
system (stats routing, jump height, grabbing rules, AI perception — all confirmed via
`reference/GameSource/Player.cs` to key off `Player.isSlugpup` directly, not `SlugCatClass`) still
correctly treats the player as a pup.

**Applied to this codebase:** `SlugpupGimmick` now patches `Player.isSlugpup`'s getter
(`MethodType.Getter` — the same pattern already proven in this codebase by `MiseryGimmick`'s
`PainJumps` hook) with a Postfix that ORs in `true` when the real character has the gimmick
assigned, and never touches `SlugCatClass`. `SlugpupStatsGimmick`'s own logic didn't need to change
conceptually (it was already keying off `playerState.slugcatCharacter`, not `SlugCatClass`) — just
one now-permanently-false guard removed (`if (self.SlugCatClass != Slugpup) return;`, which would
never pass once the flip stopped happening). `SlugpupColorGimmick` needed the least conceptual
change of the three but the most explanation: with `SlugCatClass` never flipped,
`PlayerGraphics.ApplyPalette`'s own `SlugCatClass == Slugpup` branch (the thing this class was
originally built to work around) is simply never taken anymore, so the *main body color* is already
correct for free — but `Player.ShortCutColor()` (used separately for actual shortcut/pipe-transition
tinting) branches on `!isSlugpup` in its own body, confirmed by reading
`reference/GameSource/Player.cs` directly, so once `isSlugpup` is forced `true` that one narrower
case still needs the same fix, just with its guard changed from `SlugCatClass != Slugpup` to
`!isSlugpup`.

**A second, previously-undiscovered bug found in the same pass, unrelated to the flip:**
`Player.SetMalnourished(bool, bool)` — called by vanilla any time a player's malnourished status
actually changes, a routine and frequent in-game event, not an edge case — when `isSlugpup` is true,
unconditionally replaces `npcCharacterStats` outright: `npcCharacterStats = new
SlugcatStats(SlugCatClass, flag);`. That is a fresh, un-rescaled `SlugcatStats`, discarding whatever
`SlugpupStatsGimmick`'s own `NPCStats`-constructor Postfix had carefully rescaled at spawn time — so
the very first malnourishment change after spawning would silently revert the rescale for the rest
of that cycle. This bug existed in every previous round of this gimmick (both under the old
`SlugCatClass`-flip architecture and would have persisted under the new one too) and was only found
now because this round traced `SetMalnourished` line-by-line for an unrelated reason. Fixed with a
new `SlugpupMalnourishmentStatsGimmick` (Postfix on `Player.SetMalnourished`) that re-applies the
identical rescale; the rescale logic itself was extracted into a shared `SlugpupStatsScaling.Rescale`
helper so `SlugpupStatsGimmick` and this new class can't silently drift apart from each other.

**Not yet built or tested — same container limitation as every other entry in this file (no `.NET`
toolchain here).** Every new member access (`Player.isSlugpup`'s getter, `Player.SetMalnourished`)
was checked against `reference/GameSource/Player.cs` rather than guessed, and the overall approach is
now independently validated against Pupify's own actual, working implementation rather than being a
fresh guess — but given this gimmick's track record (three real, distinct bugs found across three
separate rounds, none of them caught by compilation), don't assume this round is exhaustive either.
Next, in-game, in order: (1) it compiles; (2) Primal Gourmand, pup-ified, now shows the same visibly
fat head/body proportions a normal (non-pup) Gourmand does, just smaller — not the plain, narrow
Slugpup shape; (3) still renders in Gourmand's own tan/pink color (both main body and, more subtly,
brief shortcut/pipe-transition tint); (4) still has pup-scale physical stats (mass/speed/etc. — cross
-check the `[Slugpup] ... npcCharacterStats rescaled from real stats (...)` log line); (5) eat enough
to become malnourished (or start malnourished and eat enough to recover), then confirm the pup-scale
stats are *still* correctly rescaled afterward (not reverted to full-adult or generic-pup sizing) —
the specific case the new `SlugpupMalnourishmentStatsGimmick` patch exists for; (6) Gourmand's own
abilities/behaviors that key off `SlugCatClass == Gourmand` elsewhere in vanilla (if any are
noticeably different while pup-ified vs. not) now work correctly, matching "use that character's
abilities as a slugpup" from the gimmick's original request.

## Slugpup gimmick: ends at Outer Expanse, in detail

**The request:** "now look into a setting for the slugpup gimmick - if a given character with this
setting reaches the ending sequence in outer expanse, after the credits if they come back to the
run, they will stop being a slugpup - the gimmick will effectively be removed. its still applied,
just doesnt do anything. make this effect a default setting." Requested as a follow-up to the
SlugCatClass-flip rewrite above, but this one is a new feature, not a bugfix. Went through two
rounds of direct user correction before landing on the right mechanism — documented in full below
rather than smoothed over, per this file's own standing practice for this gimmick.

**Round 1 (wrong): hooked the ascension ending, not the standard one.** The first pass targeted
`RainWorldGame.BeatGameMode(RainWorldGame game, bool standardVoidSea)`'s `standardVoidSea: true`
branch, reasoning that its one call site (`RainWorldGame.ExitToVoidSeaSlideShow()`, from
`VoidSea/VoidSeaScene.cs`'s rising-water sequence) was "the" Outer Expanse ending. The user
corrected this immediately and specifically: **"the standard outer expanse ending is them reaching
the outer expanse region and heading all the way to the left. what you described is the void or
ascencion ending - that is why its called standardVoidSea. look for a different ending."** So
`standardVoidSea` names a *separate*, specific ascension/void-sea outcome — not "the generic Outer
Expanse ending" the name superficially suggested. Lesson: a method name that *sounds* like it
matches a user's plain-language description isn't confirmation — trace the actual room-level
behavior it's meant to describe.

**Round 2 (also wrong, but closer): hooked a room-specific class instead of the shared method,
then found it didn't generalize.** Re-traced `MoreSlugcats/MSCRoomSpecificScript.cs` for what
actually happens when a character walks to Outer Expanse's western edge. Room `"OE_FINAL03"` is
hooked unconditionally for any `IsStorySession` (no character restriction at the hookup site) and
spawns `OE_GourmandEnding` — a misleadingly-named class (presumably built for Gourmand first, then
generalized) whose `Update` tracks *any* story character's `firstChunk.pos.x` dropping below a
threshold as they walk left, explicitly special-cases `SlugcatStats.Name.White` (Survivor) inside
the same method, and only then calls `RainWorldGame.BeatGameMode(room.game, standardVoidSea:
false)`. This matched "walking to the far west of Outer Expanse" correctly, so the second version
Postfixed `OE_GourmandEnding.Update` directly (guarded by a `ConditionalWeakTable` against
re-firing every frame once `doneFinalSave` was set). **The user then asked directly whether this
generalizes: "if it gets placed on some other character, say artificer, and they beat the game with
standardVoidSea: false then that should also remove the slugpup effect. Same goes for any character
that beats standardVoidSea false."** It didn't — `OE_GourmandEnding.Update` only ever fires for the
Outer Expanse walk-ending specifically. Rivulet's Sky Islands resolution, Artificer's `LC_FINAL`,
Saint's own ascension, and Spearmaster's `SI_A07` pearl sequence all call
`BeatGameMode(..., standardVoidSea: false)` too, from completely different rooms/regions that
`OE_GourmandEnding` has no way to see.

**Round 3 (the actual fix): hook `BeatGameMode` after all, but check the right boolean.** Since the
user wants *any* "standard" (non-ascension) ending to count, regardless of which region or
character-specific room triggers it, the right hook was `BeatGameMode` all along — round 1 just had
the boolean backwards. Checking `!standardVoidSea` (rather than `standardVoidSea`) is exactly "any
standard ending," covering the Outer Expanse walk-ending, Gourmand's own alt/food-completionist
branch, and Rivulet/Artificer/Saint/Spearmaster's respective alt endings all at once, while leaving
`standardVoidSea: true` (the ascension) deliberately untouched — matching the user's own
distinction from round 1. This also turned out simpler than round 2: `BeatGameMode` is a plain
static method invoked once per ending event (not a per-frame `Update`), so the
`ConditionalWeakTable` re-fire guard round 2 needed is unnecessary here.

**Confirmed this doesn't end the save file** (matching the user's own framing, "after the credits
if they come back to the run"): none of `BeatGameMode`'s branches delete or lock the save — picking
the same character on the same slot again afterward resumes from the same den, same as vanilla's
own well-known "keep playing after the ending" behavior.

**Implementation, in three pieces, all in `Gimmicks/SlugpupGimmick.cs`:**

1. **`SlugpupEndingHook`** — a Postfix on `RainWorldGame.BeatGameMode`. Guards, in order:
   `!standardVoidSea` (skip the ascension ending entirely), `game.IsStorySession`,
   `PrimalSlot.IsActive`, the gimmick actually assigned to `game.StoryCharacter`, and the new
   setting (below) enabled for that character. If all pass, it permanently records the marker.
2. **`SlugpupEndingMarker`** — a plain existence-only marker (`"PRIMAL_SLUGPUP_ENDED<dpB>" +
   characterID`), written into the same `DeathPersistentSaveData.unrecognizedSaveStrings`
   passthrough list `OneSpearGimmick` and the Kill List gimmick already use for their own save
   markers — the established pattern in this codebase for "a fact about this save file vanilla
   itself doesn't know how to store." Unlike `OneSpearGimmick`'s *indexed, counted* markers (it
   needs to know *how many* pulls have been used), this only ever needs a yes/no, so it's a single
   un-indexed key, checked with a plain `.Contains(...)`.
3. **`SlugpupEndingSettings`** — one new `Configurable<bool> EndsOnBeatingGame`, **default true**
   (per the user's explicit "make this effect a default setting"), shown as a single `OpCheckBox`
   in the Slugpup gimmick's settings sub-view (which didn't exist before this — Slugpup previously
   had `BindSettings`/`BuildSettingsUI` both left `null`, per the "no obvious knob" note in the
   config-menu section above; that's now out of date). Named/labeled around "beats the game"
   rather than "Outer Expanse" specifically, since round 3's generalization made the narrower name
   inaccurate.

**Where the marker is actually read — and why nothing else in this file needed to change.**
`SlugpupGimmick`'s own `isSlugpup`-getter Postfix (the single choke point this whole gimmick was
already rebuilt around earlier the same day) is the *only* place that checks
`SlugpupEndingMarker.HasEnded(...)`: if the marker is present, it simply doesn't force `__result`
to `true` anymore, so `isSlugpup` goes back to returning its real, original value. Everything else
in this file already depends on that one property rather than re-deriving "is this gimmick active"
independently:
- `SlugpupStatsGimmick` only ever runs because `Player`'s own constructor gates `NPCStats`
  construction on `if (isSlugpup) npcStats = new NPCStats(this);` — once `isSlugpup` goes back to
  false, `NPCStats` (and this Postfix) simply never fires again for this player.
- `SlugpupMalnourishmentStatsGimmick` similarly only matters because `Player.SetMalnourished`'s own
  rebuild is itself gated on `if (isSlugpup)` — same story.
- `SlugpupColorGimmick` has its own explicit `if (!self.isSlugpup) return;` guard, which now
  naturally resolves to "return" once the marker is set.

So this feature needed exactly one new `if` check in one place (`SlugpupGimmick`'s own Postfix),
not four — a direct, deliberate payoff of that same-day rewrite having centralized "is this
character currently being treated as a pup" onto a single property instead of scattering separate
`SlugCatClass`/assignment checks across every patch, the way the *pre-rewrite* version would have
required touching every single class individually to get this right.

**"Still applied, just doesn't do anything," precisely:** `GimmickAssignments.IsEnabled` is never
touched by any of this — the Remix checkbox for this character stays checked forever, exactly
matching the user's own phrasing. Only the *effect* turns off, via the save marker, independent of
the assignment itself.

**Not yet built or tested — same container limitation as everything else in this file.** Every new
member access (`RainWorldGame.BeatGameMode`, `RainWorldGame.StoryCharacter`,
`RainWorldGame.IsStorySession`) was checked against `reference/GameSource/RainWorldGame.cs` rather
than guessed — all plain `public` members on an ordinary, widely-used class, not a
tightly-encapsulated simulation object, so no reflection was used (consistent with this codebase's
usual Gotcha #1 judgment call). Next, in-game, in order: (1) it compiles; (2) assign Slugpup to a
character, verify the new checkbox appears in that character's settings sub-view, defaulted to
checked, labeled around "beats the game" rather than "Outer Expanse"; (3) walk that character all
the way to Outer Expanse's western edge (not the void sea/ascension route) with the checkbox
checked, and confirm the BepInEx log shows the `[Slugpup] ... beat the game` line; (4) **assign
Slugpup to a different character (e.g. Artificer) and have them reach their own alt ending
(`LC_FINAL`) instead — confirm the same log line appears and the gimmick stops for them too, the
specific generalization the user asked to verify**; (5) continue playing either character
afterward and confirm they render/behave as their normal adult self, not a pup, for the rest of
that save; (6) uncheck the setting for a character *before* they beat the game, confirm the gimmick
keeps working afterward (the marker should never get written); (7) reach the void-sea/ascension
ending (`standardVoidSea: true`) on a Slugpup-assigned character and confirm the gimmick is *not*
affected by that specific ending, per the user's own true/false distinction; (8) beat the game a
second time on an already-ended character (if reachable) and confirm nothing breaks.

## Shared helpers and the 2026-09-15 refactor, in detail

**Why this happened.** The user asked for a full review of the codebase ("review all of the code
for potential improvements, micro and macro refactorings or even potential architectural changes")
and then for every proposal to be implemented in one pass, explicitly accepting that none of it can
be built or tested here: *"im already 15 commits deep without testing. we'll just have to test
everything at the same time, bit by bit, to bring it back probably, so a large rewrite is fine."*
So this is a large, deliberately-untested change touching almost every file — treat the whole thing
as suspect until a build and a play session say otherwise, and see Outstanding items for the
testing order that isolates each piece.

**The two real defects it fixed** (both shipping-affecting, both invisible from inside this
container):

1. **A hard dependency on SlugBase that the mod hasn't used since early 2026-09.** `Plugin.cs`
   carried `[BepInDependency("slime-cubed.slugbase")]` and `mod/modinfo.json` listed it under
   `requirements`. A bare `BepInDependency` defaults to `HardDependency`, so **BepInEx refused to
   load Primal at all unless SlugBase was also installed** — for zero functionality: nothing in the
   assembly references SlugBase in any form (only doc comments *saying* there are no SlugBase
   characters), and `Primal.csproj` doesn't even reference it. Pure leftover from the two abandoned
   custom-character designs in History below. Both declarations removed.
2. **`modinfo.json` described the abandoned design** — "Alternate versions of vanilla, Downpour, and
   Watcher slugcats with custom abilities (gimmicks), reached via a toggle on the character select
   screen", tagged `"Custom Slugcat"`. That is the design that was rejected twice, and it's the text
   players actually read in the mod manager. Rewritten to describe the reserved-slot design, tagged
   `"Gameplay"`.

**The three per-frame hot paths it fixed.** All three were introduced innocently and only became
expensive in combination with something added later:

1. **The Slugpup ending flag was being re-scanned from `Player.isSlugpup`'s getter.** The 2026-09-15
   "ends on beating the game" feature added a `SlugpupEndingMarker.HasEnded` call to
   `SlugpupGimmick`'s Postfix — which hangs off `isSlugpup`'s *getter*, a property vanilla reads
   **53 separate times in `Player.cs` alone**, many per tick. `HasEnded` built a marker string
   (allocation) and then did a linear `List<string>.Contains` over `unrecognizedSaveStrings` — a
   list the Kill List gimmick can add 55 more entries to. Gourmand with both gimmicks late in a save
   was doing a ~56-entry scan dozens of times per frame. Now: per-character marker strings are built
   once and cached, and membership goes through `PrimalSaveMarkers`' per-save-data `HashSet`. O(1),
   no allocation.
2. **`ChainsHuntedModeHook` checked the cheapest gate last.** It Postfixes
   `ArtificialIntelligence.Update` — every AI-driven creature, every realized room, every tick, so
   dozens of calls per tick in a busy room. `PrimalSlot.IsActive` sat *inside* the per-player loop,
   after resolving the room and iterating the whole player list. Hoisted to a single comparison at
   the top of the Postfix, so off-slot (and off-gimmick) it short-circuits immediately.
3. **Both threat gimmicks ran the same reflection chain independently, twice per tick.**
   `ThreatSlowdown` and `ThreatPanic` both Postfix `Player.checkInput`, and each called
   `GetCurrentThreat` (3–5 nested `FieldInfo.GetValue` calls, each boxing) for the same *global*
   number. Monk has both assigned by default. Now both read `ThreatReader.ThreatFraction`, memoized
   per `Time.frameCount` — one lookup per frame no matter how many gimmicks or players ask.

**The macro refactor: `GimmickGate`.** Every gimmick needed the same four steps in the same order
(resolve `RainWorld` → `PrimalSlot.IsActive` → real character ID → `GimmickAssignments.IsEnabled`),
and that block was copy-pasted into roughly twenty patch bodies. `MiseryEffect.IsActiveFor` had
already factored it out perfectly — but its name made it look Misery-specific, so nothing else
reused it. It's now `GimmickGate`, and `MiseryEffect.IsActiveFor` is a thin alias over it.

That consolidation also fixed a real inconsistency: **eight** gimmicks reached `RainWorld` through
`player.room.game.rainWorld` while **three** (Chains, OneSpear, Slugpup) used
`player.abstractCreature.world.game.rainWorld`. The second is strictly more robust — `Player.room`
is null while the `Player` is still being constructed, which is exactly why Slugpup and OneSpear
*had* to use it (both hook methods that run mid-constructor) — and the eight room-based ones would
have silently no-opped rather than degraded if their patched method ever fired with no room.
`GimmickGate.GameFor` tries the abstract-creature path first and falls back to `room`, so every
caller gets the robust behavior without having to know any of this.

**Everything else.** Three separate bespoke implementations of the save-marker pattern collapsed
into `PrimalSaveMarkers` (which also turned the Kill List dialog's 55 separate linear scans into 55
`HashSet` lookups); three separate cached `AccessTools.Field(KarmaLadderScreen, "saveState")`
handles collapsed into `PrimalMenuAccess`; `GimmickRegistry` now takes each ID from the owning
class's own `ID` const instead of re-typing the literal (a typo there used to be a silent, no-error
way to unwire a gimmick — all 13 were verified to match before the change); `PrimalOptions` dropped
its duplicate `_enabledConfigs` dictionary and `EnabledKey` helper in favour of
`GimmickAssignments.GetConfigurable`; and the slot-switch key became configurable
(`PrimalSettings`), shown as an `OpKeyBinder` in the Remix screen's title row.

**Watch out for, when this is finally tested:** the `PrimalSaveMarkers` cache is keyed per
`DeathPersistentSaveData` instance and validated by comparing the cached entry count against the
live list's `Count` on every read, so loading a different save or anything else mutating the list
self-heals. The one mutation it can't see is one that replaces an entry *without* changing the
list's length — nothing does that to this list in practice, but if kill-list progress or a spear-pull
count ever reads as stale, that's the first place to look.

## Survivor gimmick: community-challenge shared mechanisms (2026-09-16, branch `survivor-gimmick`)

The user's next goal: one gimmick ("Survivor") that tracks as many of the ~50 informal Rain
World community challenges as possible in a single run. Full challenge-by-challenge feasibility
pass, ratings, and the reasoning behind them: **`src/Primal/COMMUNITY_CHALLENGES.md`** (not
duplicated here). This entry only covers what actually shipped: the 8 shared mechanisms that
pass identified as covering most of the harder challenges, built on a new `survivor-gimmick`
branch, with **no per-challenge gimmick logic yet** — that was explicit scope, not an oversight.

New files, one per mechanism (see each one's own doc comment for the full decompiled-source
trace): `SurvivorEvents.cs`, `SurvivorRegionMarkers.cs`, `SurvivorShelterContents.cs`,
`SurvivorCreatureTracking.cs`, `SurvivorCreatureVariant.cs`, `SurvivorTollState.cs`,
`SurvivorCheckpointRun.cs`, `SurvivorVanillaProgress.cs`. `PrimalSaveMarkers.SaveDataFor` and a
call to `SurvivorRegionMarkers.Init()` from `RainWorldHooks.Postfix` are the only edits to
already-shipped files.

**Findings worth remembering even before any gimmick uses them:**

- **Lizard colors and Centipede's Red variant need no new mechanism at all** — RedLizard,
  CyanLizard, etc., and RedCentipede, are each their own `CreatureTemplate.Type` value in
  vanilla, the same enum Kill List already compares directly. Only BigEel/JetFish's `albino`
  bool fields and Lantern Mouse's continuous HSLColor hue are genuine runtime-variant reads —
  and the hue one was deliberately left unimplemented (no natural threshold; see
  `SurvivorCreatureVariant.cs`'s own comment).
- **A real bug in the base game's own `EntityID` struct**: `Equals` compares only `number`, but
  `GetHashCode` falls through to `ValueType.GetHashCode()` across all three fields —
  `spawner`/`number`/`altSeed` — so two EntityIDs `Equals` says are equal aren't guaranteed to
  hash equal. Using `EntityID` directly as a `Dictionary`/`HashSet` key would silently misbehave.
  `SurvivorCreatureTracking.cs` keys on the plain `int number` field instead, sidestepping it
  entirely — this applies to ANY future code in this mod that might want to key a collection on
  creature identity, not just Survivor.
- **`ScavengerOutpost`** already tracks everything Heist/Manslaughter/Elder/Usurper/Criminal
  need: `team` (guards), `worldOutpost.feePayed > 9` (vanilla's own "paid enough" threshold —
  reused, not invented), `playerTrackers[...].killOrder` (vanilla's own per-player hostility
  flag, surfaced via a Postfix on `ScavengerReportTransgression`).
- **Several challenges are just reads of state vanilla already tracks**: Sandbox unlocks
  (`PlayerProgression.miscProgressionData.GetTokenCollected`), Steam achievements
  (`RainWorld.AchievementAlreadyDisplayed` — Steam-only for now, matching the user's own
  install), Echo-met tracking (`DeathPersistentSaveData.ghostsTalkedTo`), and a kill-point-value
  table (`Menu.SandboxSettingsInterface.DefaultKillScores`, looked up per `CreatureTemplate.Type`
  via the same ExtEnum string round-trip `KillListEntry` already uses).
- **Avoid the `MoreSlugcats`-namespaced `SpeedRunTimer` HUD class** for any timing need — same
  DLC-namespace pattern that broke `PrimalOptions.AllCharacters` under MSC-disabled a few commits
  back, and every one of these challenges requires MSC disabled. `RainWorldGame.clock` (already
  used by TimeLimitGimmick) is the safe, plain-vanilla alternative; `SurvivorCheckpointRun.cs`
  uses it.
- Every new `[HarmonyPatch]` class in this batch is top-level, not nested inside the static
  helper it serves (`SurvivorWinHook`, `SurvivorFallDeathHook`, etc., alongside `SurvivorEvents`,
  not inside it) — deliberately matching every existing patch class in this codebase rather than
  introducing an untested nested-class structure Harmony's `PatchAll` has never been exercised
  against here.

**Gating note for whoever wires the actual gimmick up next:** every patch in `SurvivorEvents.cs`
and `SurvivorTollState.cs` currently gates only on the mod-wide `PrimalSlot.IsActive` check
(same as every other gimmick's cheapest-first gate), NOT on a specific gimmick assignment — there
was no gimmick ID in `GimmickRegistry` yet to check against at the time this batch was written.
Now that `CommunityChallengesGimmick.ID` exists (see the next section), re-gate these through
`GimmickGate.IsActiveFor(player, CommunityChallengesGimmick.ID)` so they only fire for a player
the gimmick is actually assigned to — still outstanding as of this doc, not done yet.

**Not yet built or tested in-game as of this doc** — no .NET toolchain in this container, and
this pass is pure infrastructure with nothing wired up to actually exercise it yet.

## Community Challenges gimmick: selection UI only (2026-09-16, same branch)

Next step on the same `survivor-gimmick` branch: the actual gimmick the shared mechanisms above
were built for, registered in `GimmickRegistry` as `CommunityChallenges` — but **settings-only,
by explicit request**: "Dont yet implement anything relating to gameplay, just tracking what the
user checks to persist across restarts." `CommunityChallengesGimmick.cs` has zero Harmony
patches. All it does: a `ChallengeEntry[]` roster of all 50 challenges (`CommunityChallenges
Registry.All`, same difficulty-ascending order as COMMUNITY_CHALLENGES.md's own table) and a
settings sub-view (`CommunityChallengesSettings`) that binds one `Configurable<bool>` per
(character, challenge) — 50 checkboxes, 5 columns of 10 rows, column-major fill, following
`ChainsSettings`' own per-type-ban checkbox grid as the closest existing precedent (same
`Dictionary<string, Dictionary<string, Configurable<bool>>>` shape, same per-character
`List<UIelement>` cache required by Gotcha #4). `settingsContentHeight: 400f` on the `GimmickDef`
gives the scroll box room for all 10 rows. **Exact pixel fit for the longest name
("Enlightenment", 13 characters) at 116px column width is unverified in-game** — flagged in the
file's own comment, same honesty as Chains' own unverified preset-menu footprint.

**Answering the user's own question, for the record:** yes, Remix config
(`ModConfigs\rokas.primal.txt`) lives entirely outside any save file, keyed only by characterID —
confirmed empirically earlier this session (a stray Kill List assignment made during live UI
testing survived a full game restart precisely because it lived there). So these 50
challenge-selection checkboxes are already save-slot-independent for free, exactly like gimmick
assignment itself already is ("if you change what gimmicks Primal Gourmand has, it should apply
to all save slots") — no new persistence mechanism was needed for that property. Actual challenge
PROGRESS, once it exists, is the opposite case and will need `DeathPersistentSaveData` (via
`PrimalSaveMarkers`), the same per-save-file mechanism Kill List's own kill records already use —
not Remix config the way these selection toggles are. Don't conflate the two when tracking logic
gets built next: selection = Remix (shared across slots), progress = save file (per slot).

**Not yet built or tested in-game as of this doc** — no .NET toolchain in this container.

## Community Challenges gimmick: completion harness, abstract tracker base, cycle-end UI (2026-09-16, later, same branch)

Before writing any of the 50 concrete trackers, the user asked for a review pass over the shared
mechanisms plus "a good abstract class to work with for all of the tracker implementations",
along with a concrete set of new behavior: a live popup+sound when a challenge completes mid-
cycle, a button above CONTINUE on the cycle-end screen showing progress, and three visual states
for that list (bold white = completed this cycle, default grey = completed earlier, darkened =
never completed) built only from the challenges checked in settings.

**Review findings, both real bugs, both fixed:**
1. `SurvivorCreatureTracking.AllTrackedAlive`'s own doc comment claimed it also required
   `realizedCreature != null` — the CODE never checked that, and confirmed via
   `AbstractCreature.Realize()` that the code was right and the comment was wrong: a creature
   routinely cycles between realized (in an active room) and unrealized (merely far from the
   player) with no relation to whether it's alive, so requiring a live body would have wrongly
   flagged a perfectly fine, just-distant creature as "not alive". Comment corrected to match the
   (correct) code.
2. The abstract `ChallengeTracker.IsEnabledFor(characterID)` this pass first drafted checked only
   `CommunityChallengesSettings.IsChallengeEnabled` — the per-challenge checkbox — not whether the
   CommunityChallenges GIMMICK ITSELF is assigned to that character. Since every per-challenge
   Configurable is bound eagerly for every real character regardless of gimmick assignment (Gotcha
   #7) and now defaults true, that check alone would have treated a character who never touched
   this gimmick as having all 50 challenges "enabled". Replaced with
   `TryGetEnabledCharacter(Player, out characterID)`, which runs `GimmickGate.IsActiveFor(player,
   CommunityChallengesGimmick.ID, out characterID)` first (the real PrimalSlot+assignment gate)
   before checking the per-challenge flag.

**New pieces, all in `CommunityChallengesGimmick.cs`** (kept in one file, matching
`CreatureKillListGimmick.cs`'s own precedent of keeping one gimmick's whole mechanism — tracker,
hooks, registry, dialog — together rather than fragmenting across files):

- `ChallengeCompletion` — permanent completion persistence (a thin `PrimalSaveMarkers` wrapper,
  same shape as Kill List's own records) plus "completed this cycle" freshness tracking for the
  summary dialog's bold-white state. Needs TWO sets, not one, because the summary dialog is
  opened on demand and might never be opened at all for a given cycle: `_accumulating` collects
  newly-completed IDs live throughout a cycle (both mid-cycle live detections and anything
  reported at Win time — both always finish before the corresponding `SleepAndDeathScreen` is
  built), and `SnapshotAndResetForNewScreen` (called once per cycle-end screen build, from
  `CommunityChallengesButtonHook`) freezes that into `_lastScreenSnapshot` and resets the live set
  — so the dialog can be opened, closed, and reopened off the same screen showing the same
  "fresh" set, while the NEXT cycle starts clean regardless of whether the button was ever
  clicked. `ReportCompletion` is idempotent: a challenge already completed on some earlier cycle
  is a silent no-op, no re-notify, no re-marking fresh.
- `ChallengeNotification` — the live mid-cycle popup+sound. Confirmed via decompiling
  `HUD/KarmaMeter.cs` that the user's own description ("a quiet sound heard after finishing a
  cycle and seeing your karma up or after eating a karma flower and the karma gets protected")
  is literally the SAME effect either way: `KarmaMeter.reinforceAnimation` plays
  `HUD_Karma_Reinforce_Flicker` → `_Small_Circle` + `_Contract` → `_Bump` on any karma-meter
  change, not exclusively the flower-shield case. Reuses only `_Bump` (the "settled" beat) rather
  than replaying the whole ~140-frame animated sequence outside of the widget it belongs to —
  which exact one of the three reads best is a judgment call, not yet confirmed by ear. The
  message itself goes through `HUD.TextPrompt.AddMessage` (vanilla's own on-screen text-popup
  mechanism, confirmed public), not a custom UI element.
- `ChallengeTracker` (abstract) — the base for all 50 future implementations. Deliberately does
  NOT standardize HOW a challenge detects completion (the 8 shared mechanisms already cover too
  many different shapes — an event subscription, a per-tick check, a plain vanilla-state read —
  for one abstract method to fit all of them); it only standardizes what happens AFTER detection:
  `TryGetEnabledCharacter` for the corrected gating above, and `ReportCompletion` to route through
  `ChallengeCompletion`. `ChallengeTrackerRegistry.All` is intentionally empty — this pass is the
  harness only, no concrete trackers yet — with `RegisterAll()` wired into `RainWorldHooks
  .Postfix` alongside `SurvivorRegionMarkers.Init()` so future entries need no other integration
  point.
- `CommunityChallengesButtonHook`/`CommunityChallengesSignalHook`/`CommunityChallengesDialog` —
  the cycle-end summary UI, built the same `AddSubObjects`/`Singal` way
  `KillListButtonHook`/`KillListSignalHook`/`KillListDialog` already are. Positioned directly
  above CONTINUE (not EXIT, where Kill List's own button sits) — confirmed via decompiling
  `Menu/KarmaLadderScreen.cs` that CONTINUE sits at `ContinueAndExitButtonsXPos - 180f -
  SafeScreenOffset.x` (EXIT is at `-320f`), so this button reuses that same X lane with the
  identical "+40f, stack directly above" offset Kill List's own button already uses one lane over.
  The dialog is text-only (no icons -- unlike creatures, challenges have no vanilla sprite to
  reuse), built only from `CommunityChallengesSettings.IsChallengeEnabled` challenges, row-major
  5-column grid (matching `KillListDialog`'s own fill order, not the settings screen's column-
  major one — no strict convention either way in this codebase yet). Bold-white uses `MenuLabel`'s
  own `bigText` flag (confirmed via decompiling `Menu/MenuLabel.cs` that this actually switches
  `Custom.GetDisplayFont()` vs `Custom.GetFont()` — there's no separate bold-weight font in this
  engine) plus `Color.white`; "completed earlier" leaves color unset (vanilla's own default read
  as "grey" already); "never completed" reuses `KillListDialog`'s own `MediumGrey` dimming.

**Also changed:** `CommunityChallengesSettings.Bind`'s default flipped `false` → `true` — "for the
time being check everything as enabled", since there's no bulk-select control on the settings
grid and nothing to actually exercise this harness against without it.

**Not yet built or tested in-game** — no .NET toolchain in this container, and no concrete
`ChallengeTracker` exists yet to actually populate a "completed" entry to look at.

## First 3 concrete ChallengeTrackers: Descension, Ranger, Spearfisher (2026-09-16, later still)

`ChallengeTrackers.cs` (new file) — three real `ChallengeTracker` subclasses, picked "at random"
per explicit request rather than deliberated over. All three turned out to need essentially no
new detection logic at all, just wiring existing pieces together:

- **DescensionTracker** — the 12-region roster (`SU, DS, HI, CC, SI, GW, SH, SL, LF, UW, SB, SS`),
  confirmed via decompiling `Menu/RegionSelectMenu.cs`'s own ordered acronym/display-name array
  pairing (its `new string[12]` of names lines up 1:1 with its own `startRooms` array's region
  prefixes) — this is the same 12-region roster every "all regions" community challenge means.
  Subscribes to `SurvivorEvents.OnFallDamageDeath`; on each fall death, checks all 12 regions
  directly (the current death's own region, plus `SurvivorRegionMarkers.HasDiedByFallIn` for the
  other 11) rather than trusting that `SurvivorRegionMarkers.Init()`'s own subscription already
  ran first and wrote the marker — both are registered from `RainWorldHooks.Postfix` in that
  order today, but not depending on it holding removes a subtle future footgun for free.
  `isLive: true` (a death is as live/real-time a moment as this mod ever sees).
- **RangerTracker** / **SpearfisherTracker** — both reduce entirely to reading Kill List's own
  already-tracked permanent record (`KillListTracker.IsKilled`) off
  `SurvivorEvents.OnSuccessfulHibernation`: Ranger counts distinct killed species across
  `KillListRegistry.All` against a 32 threshold, Spearfisher checks one specific entry
  (`CreatureTemplate.Type.BigEel`, already in the roster as "Leviathan"). `isLive: false` in both
  cases (confirmed only at Win-commit time, same as Kill List's own commit timing).

**A real cross-gimmick dependency surfaced here, worth remembering for every future kill-based
tracker**: `CreatureKillListCycleWinHook` only commits kills for a character that has the
separate "Kill List" gimmick assigned (`GimmickAssignments.IsEnabled(characterID,
CreatureKillListGimmick.ID)`), independent of whether CommunityChallenges is also assigned to
that character. Ranger/Spearfisher have no completion path at all unless BOTH gimmicks are
assigned to the same character — not a bug, just a real dependency to keep in mind.

**One deliberate simplification, flagged rather than silently assumed**: Ranger's "in one run" is
checked against Kill List's PERMANENT (whole-save-file) record, not anything scoped to a single
continuous life — this mod has no existing "run boundary" concept, and Kill List's own records
don't reset on death either. Same spirit as COMMUNITY_CHALLENGES.md's own note on Massacre/Outlaw
using "our own tally, not vanilla's".

Also added: `ChallengeTrackerRegistry.RegisterAll()` gained the same idempotent `_registered`
guard `SurvivorRegionMarkers.Init()` already has, and `CommunityChallengesRegistry.Find(id)` (a
small lookup helper every `ChallengeTracker.Challenge` property now uses instead of hand-copying
each entry's DisplayName a second time).

**Still entirely unbuilt/untested** — no .NET toolchain in this container.

## Kill tracking made global: PrimalKillTracker.cs (2026-09-16, later still, same branch)

The cross-gimmick dependency flagged in the previous section (Ranger/Spearfisher needing Kill
List separately assigned to ever see any kills) turned out to be a real architecture problem, not
just a footnote — the user's own framing: "the kill list gimmick is supposed to be more for
forcing/rewarding the character for doing all of these kills... the tracker, which is now needed
across multiple gimmicks, should be moved out as a global effect, always tracking primal
characters' kills." Fixed by literally moving the recording mechanism, not duplicating it.

**What moved, verbatim, from `CreatureKillListGimmick.cs` to the new `PrimalKillTracker.cs`:**
the `Creature.killTag` attribution Postfix on `Creature.Die`, the `PhysicalObject.Grabbed`
fallback-handler Postfix, the pending/commit/discard cycle-success/cycle-failure split, and the
`PrimalSaveMarkers`-based persistence — including the exact same save marker key
(`"PRIMAL_KILLLIST"`), kept unchanged specifically so any kill records already written during
this session's own earlier live testing round-trip correctly rather than silently going missing
under a renamed key.

**What actually changed, two things:**
1. **Gating relaxed.** Every hook now checks only `GimmickGate.TryGetCharacter` (reserved Primal
   slot, real character resolved) instead of also requiring
   `GimmickAssignments.IsEnabled(characterID, CreatureKillListGimmick.ID)` — kills are recorded
   for every Primal-slot character unconditionally now, regardless of which gimmicks (if any)
   they have assigned. `CreatureKillListGimmick.cs` itself keeps its OWN separate gate for its own
   UI (the cycle-end button still only shows if Kill List is actually assigned) — only the
   underlying recording became unconditional.
2. **The 55-species roster restriction dropped entirely from the recording step.** The original
   hooks both skipped anything not in `KillListRegistry.ByID` (now removed, it's unused); the
   global version records every `CreatureTemplate.Type` a character kills, full stop. This is
   what actually makes the mechanism usable by something like Ranger ("any 32 unique creatures",
   no restriction to a curated list) without tying its own rule to Kill List's own display
   curation. Vanilla registers ~90 `CreatureTemplate.Type` values total across every DLC, so this
   is still a small, bounded marker set per character — no unbounded-growth concern from dropping
   the restriction.

**A real ordering bug found and fixed while doing this, worth remembering for any future
Win-Prefix consumer:** `SurvivorEvents`'s own `SurvivorWinHook` and this file's `PrimalKillCommit
Hook` are BOTH separate Prefix patches on `RainWorldGame.Win` — Harmony gives no ordering
guarantee between multiple Prefixes on the same method unless one explicitly declares a priority
relative to the other, which neither does. A `ChallengeTracker` subscribed to `OnSuccessfulHibernation`
could therefore run before OR after `PrimalKillCommitHook` has actually moved this cycle's kills
from "pending" into permanent storage — reading only the permanent record would be a coin flip on
whether the cycle's own final, decisive kill (the one that pushes Ranger over 32, or the
Leviathan Spearfisher needed) is visible yet. Fixed not by forcing an order (fragile —
`HarmonyPriority` would silently need revisiting the moment a third patch on the same method shows
up) but by making `PrimalKillTracker.IsKilled`/`UniqueKillCount` both fold the in-memory pending
set into their answer directly, correct regardless of which Prefix Harmony happens to run first.
This is a general lesson, not specific to kill tracking: **any two independent Prefix (or two
independent Postfix) patches on the same vanilla method have no relative ordering guarantee in
this codebase**, and any data one of them writes should be readable in a way that doesn't assume
the other has already run.

`CreatureKillListGimmick.cs` is now purely a consumer: its own curated `KillListRegistry.All` (55
species, unchanged) plus its own cycle-end button/dialog (unchanged behavior, still gated on Kill
List's own assignment), reading `PrimalKillTracker.IsKilled` instead of a now-removed local
`KillListTracker`. `RangerTracker`/`SpearfisherTracker` (ChallengeTrackers.cs) were updated the
same way — Ranger in particular got simpler, not just fixed: `PrimalKillTracker.UniqueKillCount`
counts distinct kills directly off the save marker prefix, so it no longer needs to iterate
`KillListRegistry.All` at all.

**Still entirely unbuilt/untested** — no .NET toolchain in this container.

## 5 more ChallengeTrackers: Foe, Massacre, Mother, Sleepover, Passages (2026-09-16, later still)

Requested plainly: "build the remaining difficulty 1 or 2 trackers." Picked the ones genuinely
buildable off already-confirmed primitives, and explicitly skipped the rest of the nominal
"2 (easy)" bucket rather than force them — Visitor/Nomad/Journey/U-Turn all need a "player entered
this room" event this mod doesn't have yet (a natural next shared mechanism, given how many
pending challenges want it — not built this pass); Tasty/Leukocyte/Visions/Mycologist/Desperado
all need an "ate X" or "died while being eaten inside a den" detection this mod also doesn't have;
Outlaw needs real research into Hunter's own scoring formula (food/survival/spear-hit bonuses on
top of kills, not just kills) before a "10k points" tally means anything; Collector needs the full
SandboxUnlockID roster enumerated, which — unlike AchievementID below — is an ExtEnum, not a plain
C# enum, so `Enum.GetValues` doesn't apply and enumerating it needs its own research pass; Devotee
needs "same shelter across cycles" tracking and a firm definition of "Depths"; and Pilgrimage's
own overall rating is still dominated by cross-region carry tracking, unaffected by the earlier
color-variant-is-actually-easy finding for its "revised down" entry in this doc's own table
(re-reading that entry, it overstated things -- flagging that here rather than treating it as
settled).

- **FoeTracker** (kill all 9 Lizard types in one cycle) and **MassacreTracker** (300 kill points
  in one cycle) both needed a genuinely new PrimalKillTracker capability: "what did this
  CHARACTER kill THIS CYCLE specifically", not "ever" (what IsKilled/UniqueKillCount already
  answer). Building it surfaced a real Harmony bug worth remembering broadly: `SurvivorEvents`'
  own Win Prefix and `PrimalKillTracker`'s own commit Prefix are two independent Prefixes on
  `RainWorldGame.Win` with NO relative-ordering guarantee from Harmony. Fixed by adding
  `[HarmonyPriority(Priority.First)]` to the commit hook's Prefix -- the first actual use of
  Harmony's priority system in this codebase -- so it's now guaranteed to run before any other
  same-method Prefix that doesn't also claim a priority (every other one, as of this comment).
  That guarantee is what makes the new `WasKilledThisCycle`/`ThisCycleKillPointTotal` (both read
  a `_lastCommitted` snapshot, reset fresh at the top of every `CommitPending` call so a
  zero-kill cycle can't leak an older cycle's data forward) safe to call from
  `OnSuccessfulHibernation` subscribers. **General lesson, not specific to kill tracking: two
  independent same-type patches (Prefix+Prefix or Postfix+Postfix) on the same vanilla method
  have no ordering guarantee in this codebase unless one explicitly claims priority** -- the
  earlier IsKilled/UniqueKillCount fix worked around this by making the query itself order-
  independent (folding pending into permanent); this one solves it the other way, by forcing an
  order, because "what happened this cycle" genuinely can't be answered without one.
- **MotherTracker** (7 Noodlefly Eggs in a shelter, starting cycle) needed one new
  `SurvivorShelterContents.CountObjectsOfType` -- confirmed via decompilation that eggs are
  `AbstractPhysicalObject.AbstractObjectType.NeedleEgg`, an ITEM type distinct from any
  `CreatureTemplate.Type`, so the existing `HasLiveCreatureOfType`/`CountDistinct` (creature- and
  distinct-type-shaped) didn't fit a raw same-type instance count. "Starting cycle" reuses
  `SaveState.cycleNumber == 0`, read at Win-Prefix time (before `RainCycleTick`'s own
  `cycleNumber++` runs later inside Win's body) -- same "this cycle, not the next one" timing
  already established for other Win-Prefix reads in this codebase.
- **SleepoverTracker** (hibernate with the Red Lizard in Industrial Complex, un-killed) reuses
  `ShelterRoomFor` and a new `SurvivorShelterContents.FindLiveCreatureOfType` (`HasLiveCreatureOfType`'s
  own bool answer wasn't enough here -- checking "without taming it" needs the actual creature
  instance to inspect). **2026-09-16, later: the taming gap flagged here originally is closed.**
  Confirmed via decompiling `ArtificialIntelligence.cs` that every AI carries a public
  `friendTracker` (`FriendTracker`) whose `friend` field is the specific Creature that's tamed it
  -- null while wild -- and that `ChainsGimmick.cs` already trusts this exact same expression
  directly, twice (`lizard.AI.friendTracker?.friend == self`), to recognize its own tamed allies.
  Reused verbatim rather than re-derived: `redLizard.realizedCreature is Lizard lizard &&
  lizard.AI != null && lizard.AI.friendTracker?.friend == player`. Checked as `friend == player`
  specifically (tamed BY this character), not `friend != null` (tamed by anyone), matching the
  challenge's own wording.
- **PassagesTracker** (all vanilla Steam achievements) uses a new
  `SurvivorVanillaProgress.AllAchievements()`, enumerating `RainWorld.AchievementID` via
  `Enum.GetValues` -- confirmed via decompilation that, unlike `SandboxUnlockID`, this is a plain
  C# enum with a fixed compile-time member list, so this needed no ExtEnum-registration research
  at all. Checked once per successful cycle end (isLive: false) rather than the instant the last
  achievement pops -- no live-popup path investigated this pass.

Also: `SurvivorVanillaProgress.KillPointValue` gained a plain-`string` overload (the
`CreatureTemplate.Type` one now just delegates to it) since `PrimalKillTracker
.ThisCycleKillPointTotal` only ever has a killed creature's type as a string already and had no
reason to reconstruct a full `CreatureTemplate.Type` just to hand back to this file.

**Still entirely unbuilt/untested** — no .NET toolchain in this container.

## 5 more shared mechanics realized: room transitions, eating, generalized death, passages, demasking (2026-09-16, later still)

Requested plainly: "realize 5 of the first and most useful shared mechanics not yet realized" —
referring to COMMUNITY_CHALLENGES.md's own 19-item gap list a few sections up. Picked #1, #2, #3,
#6, #7 from that list (highest-leverage genuinely-unbuilt ones; #4 and #5 were skipped on purpose
— both are already-built mechanisms with zero consumers, not "not yet realized"). All 5 landed as
new events + Harmony patches on the existing `SurvivorEvents` bus (`SurvivorEvents.cs`), each
following that file's own established shape exactly: one top-level `[HarmonyPatch]` class per
vanilla hook point, an `Action<...>` event plus `Raise*`/`Has*Subscribers` pair, gated the same
broad way the original four events already are (no per-gimmick gating here — that's still each
future `ChallengeTracker`'s own job via `TryGetEnabledCharacter`).

- **`OnPlayerEnteredRoom`** — Postfix on `Player.NewRoom(Room)`. Confirmed via decompilation to
  be Player's own override of the virtual `PhysicalObject.NewRoom` — the same "just arrived"
  moment vanilla itself uses (tongue repositioning, MSC's item-tracker spawn logic, transition-
  ripple reassignment). Hands back room name (`AbstractRoom.name`) and region name (`World.name`)
  as plain strings rather than the Room/AbstractRoom objects, since every planned consumer
  (`SurvivorCheckpointRun.NotifyRoomEntered`, future region-visited trackers) only keys off the
  name anyway. This is the one `SurvivorCheckpointRun` (built earlier, zero consumers) was
  waiting on.
- **`OnObjectEaten`** — Postfix on `Player.ObjectEaten(IPlayerEdible)`. Confirmed via 14 separate
  call sites across the decompiled source (KarmaFlower, Mushroom, DangleFruit, Centipede,
  EggBugEgg, Fly, Hazer, JellyFish, OracleSwarmer, SLOracleSwarmer, SlimeMold, SmallNeedleWorm,
  SwollenWaterNut, VultureGrub — each calls `(grasp.grabber as Player).ObjectEaten(this)` on
  itself once fully eaten) to be THE universal "finished eating this" hook. Hands back the raw
  `IPlayerEdible` — that interface alone exposes no type info (just `BitesLeft`/`FoodPoints`/
  `Edible`/`AutomaticPickUp`), and its implementers span both items and creatures (e.g. Hazer), so
  each subscriber pattern-matches for whatever specific thing it cares about.
- **`OnPlayerDeath`** — the "gap #3" generalization of the fall-death pattern: a Postfix on
  `Player.Die()` itself, using the identical wasDead/isDead transition trick
  `SurvivorFallDeathHook` already proved (see that hook's own comment). Confirmed via
  decompilation that Player.Die() is Player's own override of the virtual `Creature.Die()` and —
  outside an MSC/Coop-only branch this mod doesn't target — does NOT call `base.Die()`, which
  means `PrimalKillDieHook`'s existing patch on the BASE `Creature.Die` never actually fires for a
  player's own death; this new hook is the only one that sees it. Reads killer creature type off
  the same `PrimalKillTracker.KillTagField` reflected handle (no second reflection lookup needed —
  promoted no further, just reused) and hands back `AbstractWorldEntity.InDen` and
  `AbstractRoom.shelter` as the two location flags Tasty ("pulled into a den") and Leukocyte
  ("died inside a shelter") each need.
- **`OnPassageUsed`** — a Prefix/Postfix pair on `Menu.SleepAndDeathScreen.Singal`, mirroring the
  exact body of its own `"PASSAGE"` case (confirmed via decompilation: only actually calls
  `endgameTokens.Passage(...)` if `proceedWithEndgameID` was null on entry AND
  `winState.GetNextEndGame()` returned non-null) — fires only on that real null→non-null
  transition, the same before/after-state trick used everywhere else in this file. **Deliberate
  simplification, flagged on purpose:** doesn't distinguish which passage was taken, just that one
  was — a conservative reading that happens to be the correct, stricter one for Enlightenment's
  "without ever passaging" condition. No live `Player` exists on this screen; characterID comes
  from `PrimalMenuAccess.GetCharacterID`, the same menu-side identity path
  `CommunityChallengesButtonHook`/`KillListButtonHook` already use.
- **`OnNonLethalDemask`** — a Postfix on `Vulture.Violence` comparing
  mask-before/mask-after/dead-after, NOT a patch on `Vulture.DropMask` itself: decompilation
  confirms `DropMask` (Vulture.cs:1624) has exactly one call site in the whole source, inside
  `Violence`, before `Violence`'s own trailing `base.Violence(...)` call actually applies the
  damage that could kill the creature — so patching `DropMask` directly would run too early to
  know if that same hit turned out fatal. Attribution reuses `PrimalKillTracker.KillTagField`
  rather than parsing `Violence`'s own `BodyChunk`-ownership chain: `Creature.SetKillTag` is
  called by `Weapon`'s own hit-resolution code on every connecting hit, not just fatal ones
  (`Weapon.cs`'s projectile-collision handling calls it unconditionally on any `Creature` result),
  decaying only after 200 ticks (`killTagCounter`) if the creature survives — so `killTag` is
  already exactly "who most recently hit this creature" at the moment `Violence`'s Postfix runs,
  the same signal kill-tracking already trusts for lethal hits. `Vulture.VultureState.mask`
  (a plain public bool) is trusted directly rather than reflected, matching the direct-trust
  precedent already set for `ArtificialIntelligence.FriendTracker.friend` in `ChainsGimmick.cs`.
  **Known gap, flagged on purpose:** this is ONLY the mask-falls-off-a-live-Vulture mechanic
  (Usurper's need). Elder's "6 Scavengers simultaneously wearing Vulture Masks" is a completely
  different, not-yet-researched mechanic — Scavengers picking up and equipping a `VultureMask`
  item as decoration, unrelated to `Vulture.Violence`/`DropMask` at all.

**Zero consumers wired up for any of these 5** — matches the original 8-mechanism batch's own
"mechanism only, no gimmick logic yet" scope. Brace/paren balance re-checked across the whole
`src/Primal` tree after this change (all files balanced). **Still entirely unbuilt/untested** — no
.NET toolchain in this container; every vanilla member used above was verified against
`reference/GameSource/*.cs` first, per this project's standing rule.

## Original 50-challenge table recovered as CSV; 6 more trackers wired (2026-09-16, later still)

Requested: "wire up the trackers for whom the shared mechanics already exist for." Hit a real
blocker immediately: COMMUNITY_CHALLENGES.md's own ratings table only ever recorded a condensed
"what it needs" summary per challenge — the *mechanism* shape, not always the concrete data
(exact creature type, room name, threshold) needed to implement one correctly — and the original
50-row table with that data had been pasted in chat once, then fell out of context on
compaction. Rather than guess specifics into permanent game logic, asked the user directly; they
re-pasted the full table, now saved verbatim as `src/Primal/community_challenges_source.csv` (51
rows including header, 23 columns) for this and every future batch to reference without needing
another re-paste.

With exact wording available, picked exactly the challenges buildable from mechanisms that
*already exist* (no new mechanism research — matching the request's own scope) and skipped
everything still blocked on something genuinely new: cross-region creature-carry (Chieftain,
Fishtank, Symbiote, Charged, Offering, Chef, Guardian, Pilgrimage), toll/reputation state wiring
(Manslaughter, Heist, Elder, Criminal), a color/variant roster (Feast, Friend, Safari, Scholar),
and challenges needing room-name data-entry the CSV doesn't actually give either (Nomad, Journey,
Circuit, Desperado, U-Turn, Visitor — their descriptions name real vanilla rooms/regions, but not
by internal room ID, which would need a separate lookup pass, not just wiring).

Six landed, added to `ChallengeTrackers.cs` and registered in
`ChallengeTrackerRegistry.All` (`CommunityChallengesGimmick.cs`):

- **`TastyTracker`** / **`LeukocyteTracker`** — both direct consumers of the new
  `SurvivorEvents.OnPlayerDeath` (killer type + den/shelter flags), exactly what that event was
  built for. Tasty ("pulled into a den and eaten by a centipede") counts all three vanilla
  centipede `CreatureTemplate.Type`s (`Centipede`, `SmallCentipede`, `RedCentipede`, confirmed
  distinct via decompiling `CreatureTemplate.cs`) rather than guessing the community means only
  one specific size — a judgment call, flagged in the class's own doc comment. Leukocyte ("eaten
  by a Daddy Long Legs inside of a shelter") is a single killer type + the `inShelter` flag.
- **`VisionsTracker`** (10 Karma Flowers in one cycle) / **`MycologistTracker`** (a Mushroom from
  4 distinct regions on the starting cycle) — both direct consumers of the new
  `SurvivorEvents.OnObjectEaten`. `KarmaFlower`/`Mushroom` are plain vanilla classes implementing
  `IPlayerEdible` directly, so `edible is KarmaFlower`/`is Mushroom` needs no reflection. Each
  keeps a small per-character, in-memory counter/set (matching `PrimalKillTracker`'s own
  in-memory-table precedent), reset on both `OnSuccessfulHibernation` and `OnCycleFailed` since
  either ends "this cycle" for the next one to start fresh. Mycologist's cycle-0 gate reuses
  `SaveState.cycleNumber == 0` exactly as `MotherTracker` already established, just checked at
  the moment of eating instead of at Win time (both are safe for the same reason: `RainCycleTick`'s
  own increment only ever happens inside a successful `Win` call, never mid-cycle). **Known gap,
  flagged explicitly:** Visions' own "No Hunter Flower!" exclusion isn't checked — no vanilla flag
  for a Hunter-specific Karma Flower turned up on a first pass, and confirming one exists needs
  its own research, out of this batch's "wire up existing mechanics" scope.
- **`UsurperTracker`** — the first consumer of the new `SurvivorEvents.OnNonLethalDemask`. Two
  conditions, both checked: a per-cycle count of King-Vulture-specific demasks (`isKing` flag on
  the event) reaching 2, AND at hibernation, at least 2 King Vulture masks physically present in
  the shelter. The king/non-king distinction reuses `VultureMask.AbstractVultureMask.king` — a
  plain public bool (confirmed via decompiling `VultureMask.cs`, set from the same `IsKing` flag
  `Vulture.DropMask` already passes when constructing the dropped mask) — trusted directly, the
  same direct-trust precedent already set for `VultureState.mask` in `SurvivorEvents.cs` itself.
  Reads shelter contents via `SurvivorShelterContents.EntitiesInShelter` directly rather than
  extending `CountObjectsOfType` (whose `AbstractObjectType`-equality signature has no way to
  filter on the `king` instance field). **Known gap:** "No Echo pacification!" left unenforced,
  an honor-system note.
- **`PerseveranceTracker`** — the first consumer of `SurvivorEvents.OnGameEnding` at all (closing
  gap-list item #19, "zero-cost, already built" since the original 8-mechanism batch — literally
  nothing new needed here beyond writing the subscriber). Reuses the exact same `standardVoidSea`
  flag `SlugpupGimmick`'s own ending hook already relies on (`SlugpupEndingHook`,
  `SlugpupGimmick.cs`), iterating `game.Players` the same way `SurvivorGameOverHook`/
  `SurvivorWinHook` already do (`OnGameEnding` itself only hands back `(RainWorldGame,
  standardVoidSea)`, no `Player`, unlike every other `SurvivorEvents` event). **Known gap:**
  "starting on cycle zero" and "no Guardian skip" are both Remix-assist-configuration/
  honor-system clauses per the CSV's own Notes column ("Hunter cycles must be set to 1 and Bonus
  cycles to 0 via remix assists"), not in-run state this mod can actually check — left
  unenforced, same treatment Saint/Vegan's own unenforceable clauses already get.

14 of 50 challenges now have a tracker registered (up from 8). **Still not built or tested in a
real game** — no .NET toolchain in this container. Brace/paren balance re-checked across the
whole `src/Primal` tree after this change.

## Cross-region carry, toll state, and color variant mechanisms (2026-09-16, later still)

Requested: "implement shared mechanics for cross region creature carry, toll state and color
variant identification" — the three biggest remaining blockers on COMMUNITY_CHALLENGES.md's
shared-mechanisms list (items #3, #5, #4 there). Mechanism-only, same scope as every earlier
shared-mechanics batch in this doc — no new `ChallengeTracker`s wired up this pass.

**Cross-region carry — one method added to `SurvivorCreatureTracking.cs`.** The identity-tracking
half (`Touch`/`Untrack`/`AllTrackedAlive`/`FindInRoom`) already existed with zero consumers, built
during the original 8-mechanism batch. Its own header comment already correctly diagnosed the
actual gap: it can tell you IF a tracked creature is alive and WHERE it last was, but not whether
the player is currently holding it. Closed with `IsBeingCarriedBy(Player, AbstractCreature)`,
reading `Creature.grasps` (confirmed public property, `Grasp[] grasps { get; set; }`) — the same
array-of-`Grasp` structure `PrimalKillTracker`'s own `Grabbed` hook already reads from the
grabber's side (`grasp.grabber is Player`), just walked from the player's own `grasps` this time
to ask "is `creature`'s realized body one of the things I'm holding." Deliberately requires the
tracked creature to be realized — unlike `AllTrackedAlive` (where "not realized" would wrongly
read as dead), here it's correct: a creature can't be in anyone's grasp while merely abstract, so
"not realized" really does mean "not being carried right now." Combined with the already-built
`SurvivorEvents.OnPlayerEnteredRoom`, this closes out every genuinely-carry-shaped challenge
(Symbiote, Charged, Offering, Chef, Guardian, Pilgrimage): track the creature's identity at pickup,
check `IsBeingCarriedBy` + `AllTrackedAlive` at each room arrival, done. Chieftain's "bring live
Scavengers" and Pilgrimage's own Lizard-lineage half turned out to be a DIFFERENT case on closer
reading of the CSV — an escorted/tamed creature that follows the player on its own rather than
being grasped — which needs no carry check at all, just the identity-tracking + room-arrival pair
already built; flagging this so a future pass doesn't waste time trying to detect a "carry" that
was never going to happen for those two.

**Toll state — `SurvivorTollState.cs` extended, plus a new `SurvivorTollRoster.cs`.** That file's
own header comment had already flagged one open gap from the original build: no way to tell
"stole a pearl" apart from "attacked a Scavenger," since both funnel through
`ScavengerOutpost.ScavengerReportTransgression(Player)`, whose own signature carries neither
detail. Traced the REAL call sites this time (`ScavengerAI.cs`): both live inside
`ScavengerAI.SocialEvent(SocialEventRecognizer.EventID ID, Creature subjectCrit, Creature
objectCrit, PhysicalObject involvedItem)`, which has exactly what's missing — the specific event
type and the actual item — before it ever decides whether the hostility threshold
(`num > 0.5f`, a mix of relationship state/distance/item value not worth re-deriving) is crossed.
Rather than force that decision to happen through `ScavengerReportTransgression`, added
`OnItemStolen` as its own independent signal straight off `SocialEvent` whenever `ID ==
EventID.Theft` — for Heist's "stole a pearl without paying," the item changing hands at all is the
real signal, regardless of how offended the Scavenger got about it. Also added `OnTollPassed`, a
SECOND, independent Postfix on `Player.NewRoom` (the same vanilla method
`SurvivorEvents.OnPlayerEnteredRoom` already patches on its own — two unrelated Postfixes on one
method, no ordering dependency between them since neither reads the other's result) that fires
once a player's room changes away from a room that had a `ScavengerOutpost`, handing back
`HasPaidToll` at the moment of departure — Criminal's own "passed 3 tolls without paying" need. A
dedicated hook rather than reshaping `OnPlayerEnteredRoom`'s own signature, since that event only
ever hands back plain name strings by design, not the `Room` object `SurvivorTollState.OutpostIn`
needs.

Manslaughter needed something genuinely new: "every Scavenger EVER assigned to this toll died,
with zero player kill credit and zero hostility, ever." `ScavengerOutpost.team`
(`SurvivorTollState.GuardsOf`) can't answer that by itself — confirmed via decompiling
`ScavengerOutpost.Update`'s own body that a dead/untrackable Scavenger is removed from `team` the
SAME tick (`ScavToBeTracked` check, `team.RemoveAt`), and a replacement guard just gets appended
in its place. By the time the last guard dies, every earlier casualty has already fallen out of
the list — `team` only ever shows who's CURRENTLY there, never who was EVER there. Built a new
file, `SurvivorTollRoster.cs`, keeping its own persistent roster keyed on the outpost's own room
name (`AbstractRoom.name` — stable across room reloads, unlike the `ScavengerOutpost` instance
itself, which is recreated fresh every time the room loads): a `SurvivorTollRosterObserveHook`
Postfix on `ScavengerOutpost.Update` records every currently-tracked guard's identity
(`EntityID.number`, per the same Equals/GetHashCode gotcha `SurvivorCreatureTracking`'s own header
comment documents) and refreshes each one's dead/alive status every tick; a separate
`SurvivorTollRosterDeathHook` Postfix on `Creature.Die` (independent of `PrimalKillDieHook`'s own
unrelated Postfix on the same method — again, no ordering issue, neither reads the other) checks,
the instant any Scavenger dies, whether `PrimalKillTracker`'s own killTag/handler attribution
credits the kill to a gated player, and if so permanently disqualifies that outpost's roster.
`SurvivorTollRoster.AllEverSeenGuardsDead(roomKey)` and `.PlayerEverCreditedAGuardKill(roomKey)`
are the two queries a future Manslaughter tracker will actually read.

**Color/variant identification — one confirmation, one write-up, in `SurvivorCreatureVariant.cs`.**
Confirmed `CreatureTemplate.Type.SpitterSpider` is its own distinct registered Type (same as
`RedLizard`/`RedCentipede`/`CyanLizard` already were) — closes out Safari's third creature; all
three of Red Lizard/Red Centipede/Spitter Spider are now plain Type comparisons, no mechanism
needed at all. Revisited the Lantern Mouse purple/green question the file had previously deferred
without tracing the actual formula — this pass read `LanternMouse.GenerateIVars` directly: hue is
a 1%-chance fully-random roll, else roughly 50/50 between `Lerp(0.5, 0.65, random)` (a
cyan-through-violet arc, the most plausible source of "purple") and `Lerp(0, 0.1, random)`
(red/orange — NOT green). There is no dedicated green cluster in the ordinary case at all; true
green (~0.33 hue) only ever arises from that rare 1% wildcard. This confirms — doesn't just
assume — that the earlier decision to leave this unimplemented was correct: it's a continuous
float with two common clusters and a rare wildcard, not a clean two-value split, exactly the kind
of judgment call the challenge's own rules defer to a human colour-picker for. Left unimplemented,
now with the real formula on record instead of a placeholder "haven't looked" note.

**Still not built or tested in a real game** — no .NET toolchain in this container. Every vanilla
member used (`Creature.grasps`, `ScavengerAI.SocialEvent`, `ScavengerAI.outpostModule`,
`ScavengerOutpost.Update`/`team`, `CreatureTemplate.Type.SpitterSpider`,
`LanternMouse.GenerateIVars`) was verified against `reference/GameSource/*.cs` first. Brace/paren
balance re-checked across the whole `src/Primal` tree after this change.

## Color-variant trackers: Feast and Safari (2026-09-16, later still)

Requested: "implement the color variant mechanic needed trackers." The color-variant mechanism
was originally rated against six challenges (Feast, Safari, Sleepover, Offering, Chef,
Pilgrimage) — Sleepover was already done. Checked the other five against the exact CSV wording
before assuming the variant check alone was enough to unblock them:

- **Feast** and **Safari** really were blocked only on the variant check, now resolved. Both
  implemented.
- **Offering, Chef, Pilgrimage** turned out to each need something ELSE the color-variant
  mechanism doesn't provide, despite being on the original list:
  - Offering: Lantern Mouse purple/green (deliberately left unimplemented — see this doc's own
    "5 more ChallengeTrackers"/color-variant sections above) AND a wholly separate, unbuilt
    "fed a carried creature to a predator" event. Two blockers, not one.
  - Chef: albino Jetfish is already covered, but "kill it on a zapper" needs death-by-hazard-
    object detection — a Zapper is an environmental object, not a `Creature`, so none of the
    killTag-based mechanisms (`OnPlayerDeath`, `PrimalKillTracker`) apply to it at all.
  - Pilgrimage: Cyan Lizard is trivial (already a confirmed Type, already used by `FoeTracker`),
    but its route ("Memory Crypts, The Leg and Underhang") doesn't resolve to real region
    acronyms. Checked the authoritative source this pass — `Menu/RegionSelectMenu.cs`'s own debug
    region list (`regions = new string[12, 2]`): SU="SUBURBAN", HI="HEAVY INDUSTRIAL",
    DS="DRAINAGE SYSTEM", CC="CHIMNEY CANOPY", LF="LINEAR FARMS", SI="SKY ISLANDS",
    SH="SHADOW URBAN", GW="GARBAGE WASTES", SL="SHORELINE", UW="THE WALL", SB="SUBTERRANEAN",
    SS="SUPER STRUCTURE". UW="THE WALL" nicely confirms Pilgrimage's own "No climbing The Wall!"
    note names a real region on its route — but "Memory Crypts," "The Leg," and "Underhang" match
    none of these 12 official names, so they're almost certainly community nicknames for specific
    SUB-areas within one or more of these regions, not resolvable from what's in this repo. Same
    category of gap as Nomad/Journey/Circuit/Desperado/U-Turn/Visitor from the earlier
    "6 more trackers" batch — room/sub-area name data-entry, not a missing mechanism.

All three skipped, each for a reason that ISN'T the color-variant mechanic (flagging this so a
future pass doesn't re-attempt them expecting the variant check alone to be the unlock).

**`FeastTracker`** ("Lineage, kill and eat one Red Centipede") needed one small addition to
`PrimalKillTracker` itself: "Lineage" means the SAME creature instance must be both killed and
eaten, not "killed one and separately ate a different one" — every existing record in that class
(`_pending`, the permanent marker set) is per-TYPE, which can't answer an instance-level question.
Added `_instanceKills` (`Dictionary<string, HashSet<int>>`, keyed on `EntityID.number` per the
usual Equals/GetHashCode gotcha) plus `RecordInstanceKill`/`WasInstanceKilledBy`, populated from
the SAME existing `PrimalKillDieHook` Postfix — no new Harmony patch needed at all, just one more
line in an already-firing hook. `Centipede` implements `IPlayerEdible` directly (confirmed via
decompilation — a dead Centipede's own corpse is what eventually reports being eaten through
`Player.ObjectEaten`), so `FeastTracker` itself is just: on `OnObjectEaten`, is this a Centipede of
type `RedCentipede`, and does `WasInstanceKilledBy` recognize its specific `abstractCreature.ID
.number`. A direct correlation of two already-correct signals, not an approximation.

**`SafariTracker`** ("Red Lizard, Red Centipede and Spitter Spider alive on the same screen, then
sleep") reads the player's CURRENT room's entities directly (`player.abstractCreature.Room
.entities`) rather than `SurvivorShelterContents` (which is deliberately shelter-only and would
never see this outside a shelter room) — checked once per room arrival
(`SurvivorEvents.OnPlayerEnteredRoom`), setting a per-character, per-cycle "seen all three alive
together" flag that's read (and cleared) at the next `OnSuccessfulHibernation`, or cleared outright
on `OnCycleFailed`. "Red Lizard spit doesn't count as on-screen" needed no special handling — this
checks real `AbstractCreature` entities of each type directly, never anything a spit projectile
could be confused with. **Known simplification:** only re-checked on the player's OWN room
transitions, not continuously — a third creature wandering into an already-occupied room while the
player stays put won't be caught until the player's next room change.

16 of 50 challenges now have a tracker registered (up from 14). **Still not built or tested in a
real game** — no .NET toolchain in this container. Brace/paren balance re-checked across the whole
`src/Primal` tree after this change.

## Pilgrimage, Chef, Offering: the last 3 unimplemented trackers (2026-09-16, later still)

Requested: "implement mechanics needed for these 3 unimplemented trackers," together with three
pieces of user-supplied research that turned out to be exactly right and unblocked each one:
"Memory Crypts" is a subregion of Shaded Citadel, "The Leg"/"Underhang" are subregions of The
Exterior; Chef's zap-coil-proximity idea; and Offering's hint that "fed to a predator" should hook
the same code as taming/Lizard-reputation increases.

**Pilgrimage.** Confirmed the region acronym mapping against the authoritative source —
`Menu/RegionSelectMenu.cs`'s own debug region list (`regions = new string[12, 2]`): Shaded
Citadel = SH ("SHADOW URBAN" in that list), The Exterior = UW ("THE WALL" — which also explains
Pilgrimage's own "No climbing The Wall!" note, since UW is genuinely on this route). Primal has no
sub-area/room-cluster data below the whole-region level (`World.name` is the finest grain
anywhere in this codebase), so `PilgrimageTracker` tracks at REGION granularity instead: start
tracking the first time this character is seen carrying a live Cyan Lizard while in Outskirts
(SU), record SH and UW as "visited" (in either order) while the lizard stays alive and carried,
and complete on arrival in Five Pebbles (SS) once both are recorded. No new mechanism needed at
all — `SurvivorCreatureTracking`, `IsBeingCarriedBy` (added earlier this session for exactly this
shape of challenge), and `SurvivorEvents.OnPlayerEnteredRoom` were already sufficient; this was
purely a region-name research gap, closed by the user directly.

**Chef.** The user's own zap-coil-proximity hunch turned out to be not just a reasonable proxy but
the ONLY viable approach: decompiling `ZapCoil.cs` (`ZapCoil.Update`'s own body) confirmed it
kills a creature with a DIRECT `Creature.Die()` call the instant a body chunk touches its contact
zone while `turnedOn > 0.5f` — completely bypassing `Violence`/`Creature.SetKillTag`. There is
structurally no killer `AbstractCreature` to attribute this death to at all, so every existing
killTag-based mechanism in this mod (`PrimalKillTracker`, `SurvivorEvents.OnPlayerDeath`) could
never have seen this cause no matter how it was extended. Built a new file,
`SurvivorHazardDeath.cs`: `Room.zapCoils` (confirmed public `List<ZapCoil>`, maintained by `Room`
itself) is checked for at least one coil with `turnedOn > 0.5f` — the exact same threshold
`ZapCoil.Update`'s own real zap logic gates on — at the moment a creature dies in that room, fired
as `OnZapCoilDeath(Creature)` off an independent Postfix on `Creature.Die` (a THIRD independent
Postfix on that method now, alongside `PrimalKillDieHook` and `SurvivorTollRosterDeathHook` — no
ordering needed, none of the three read each other's results). **Known imprecision, flagged
explicitly:** this can't prove the coil caused THIS specific death, only that one was active in
the same room — accepted as good enough for Chef's own deliberate, curated setup (the player
chooses exactly which creature to bring to exactly which zapper). `ChefTracker` itself: carry-track
an albino Jetfish from Subterranean (SB), gate `OnZapCoilDeath` on the death happening in Five
Pebbles (SS) specifically and the creature being the tracked instance (a small reverse index,
creature-instance-number → characterID, since the death event hands back only the creature), then
require the SAME instance be eaten afterward (reusing FeastTracker's own "corpse is what
`OnObjectEaten` reports" pattern, this time with `JetFish` instead of `Centipede`).

**Offering.** The user's hunch about the taming/reputation code was directionally exactly right —
that system is real (`SocialEventRecognizer.ItemOffered`, raising `EventID.ItemOffering`, handled
by `LizardAI`/`CicadaAI`/`ScavengerAI`'s own `SocialEvent` methods) — but confirmed via decompiling
`BigEelAI.cs` that it does NOT apply to Leviathans: `BigEelAI` does not implement
`IReactToSocialEvents` at all, unlike every creature that actually does react to offerings, so
`SocialEventRecognizer.SocialEvent`'s own dispatch loop (which filters on exactly that interface)
structurally never reaches a BigEel regardless of what's offered to it. The real mechanism is
`BigEel.Swallow()`, confirmed via decompiling `BigEel.cs` to be the one method that actually
consumes whatever the Leviathan has caught: it iterates its own `clampedObjects` (public
`List<ClampedObject>`, each with a public `chunk.owner`) and calls `.Destroy()` on each, then
clears the list — independent of Violence/killTag/SocialEventRecognizer entirely, same shape of
"autonomous predator kill" as Chef's zap coil. Built a new file, `SurvivorPredation.cs`, with a
Prefix (not Postfix — `Swallow`'s own body immediately destroys and clears everything, so a
Postfix would find nothing left) on `BigEel.Swallow`, firing `OnLeviathanSwallow(BigEel, Creature)`
for each caught creature before it's destroyed.

For the color half, the user explicitly asked to stop deferring the Lantern Mouse hue question
("you should be able to find the hue") — overriding the earlier honor-system decision (kept in
`SurvivorCreatureVariant.cs`'s own comment for context, since the underlying uncertainty it
describes is still real). Added `IsPurpleOrGreenLanternMouse`, bucketing the already-traced
`GenerateIVars` formula: PURPLE = the formula's own common `[0.5, 0.65]` cool-color cluster (not
textbook HSL purple, but the closest match to what the community actually sees given this
formula's fixed saturation/lightness), GREEN = the standard HSL green band `[0.25, 0.45]` — only
reachable via the formula's rare 1% fully-random roll, since neither of its two common branches
ever lands there. `LanternMouse.iVars`/`IndividualVariations.color`/`HSLColor.hue` are all
confirmed public fields, trusted directly per the same precedent as `VultureState.mask`.

`OfferingTracker` itself: carry-track a matching Lantern Mouse (any starting region — only the
DESTINATION, Subterranean, is named), gate `OnLeviathanSwallow` on the eel being confirmed albino
and the swallow happening in Subterranean, and check the SAME tracked instance was swallowed (a
reverse index again, this time keyed on the mouse's instance number → the player's own
`AbstractCreature`, since `ReportCompletion` needs an actual live `Player` and re-resolving
`playerAbstract.realizedCreature as Player` at swallow time is simpler than a characterID-only
index here). "Survive the encounter" is checked as "the player is not ALSO dead at the exact
moment of the swallow" — a deliberate simplification, not a longer post-encounter survival window,
flagged as such in the tracker's own doc comment.

All three of this batch share one shape-of-problem: their terminal event (`OnZapCoilDeath`,
`OnLeviathanSwallow`) hands back only the affected creature, never a player, because the predator
in question kills/eats autonomously — there is no player-side hook at that exact instant to piggy-
back on the way `OnObjectEaten`/`OnPlayerDeath` naturally provide one. Each tracker solves it with
its own small, self-contained reverse index rather than changing either event's own signature (a
generic index would need to live somewhere shared, and no second consumer exists yet to justify
that).

19 of 50 challenges now have a tracker registered (up from 16) — every challenge originally on the
color-variant list, and both remaining carry-mechanism/toll-adjacent challenges from the last
research pass, are now implemented. **Still not built or tested in a real game** — no .NET
toolchain in this container. Every vanilla member used (`Room.zapCoils`, `ZapCoil.turnedOn`,
`BigEel.Swallow`/`clampedObjects`/`ClampedObject`, `BigEelAI` confirmed NOT implementing
`IReactToSocialEvents`, `LanternMouse.iVars.color.hue`, `Menu/RegionSelectMenu.cs`'s own debug
region list) was verified against `reference/GameSource/*.cs` first. Brace/paren balance
re-checked across the whole `src/Primal` tree after this change.

## Ornaments implemented; Collector/Devotee/Martyr/Guardian researched, need user input (2026-09-16, later still)

Requested: "go through 5 more gaps" from the 19-item shared-mechanics list, with an offer to help
with mechanical/gameplay knowledge or explain challenges in more detail. Picked 5 of the remaining
items (#8, #10, #12, #13, #15 — #1-7 and #19 were already closed): one resolved cleanly from
decompiled source alone, four hit genuine ambiguity flagged explicitly rather than guessed past.

**Ornaments (item #12) — implemented.** "Pin 12 unique, live creatures against any surface in a
single cycle." Traced `Spear.LodgeInCreature` (the method that runs when a thrown spear embeds in
a creature): if `spearDamageBonus > 0.9f` and there's a solid wall directly behind the creature in
the throw direction, it sets `stuckInWall` (`Vector2?`) and `pinToWallCounter = 300` — the real,
literal "pinned to a surface" mechanic, both plain public fields on `Spear`. Built
`SurvivorPinning.cs`: a Prefix/Postfix pair on `LodgeInCreature` using the standard wasX/isX
transition trick (comparing `stuckInWall.HasValue` before/after, not checking `== 300` directly,
so nothing depends on 300 being a value nothing else could coincidentally produce), firing
`OnCreaturePinnedToWall(Spear, Creature)`. `OrnamentsTracker` counts distinct creature INSTANCES
(not species — the CSV's own wording never mentions species diversity the way Foe's "9 Lizard
types" does) that were alive at the exact moment of pinning, reset per cycle; "Creatures don't
need to be left pinned" (confirmed in the CSV's own Notes column) is exactly why this counts the
instant of pinning, not any ongoing pinned-state check.

**The other four, each blocked on something this doc's own source-reading can't resolve alone:**

- **Collector (item #13)** — the ENUMERATION mechanism turned out to be trivial once found:
  `ExtEnum<T>.values.entries` is a plain public `List<string>`, the full registered-value roster,
  directly readable with zero reflection (simpler than `AllAchievements()`'s own `Enum.GetValues`
  approach, since `SandboxUnlockID` isn't a plain enum). What's actually unresolved is SCOPE: does
  "all arena, creature and item Sandbox unlocks" mean literally every registered
  `SandboxUnlockID`, or a curated subset — the challenge names three categories specifically,
  which might imply excluding some other unlock kind this pass never identified. Needs either the
  user's knowledge of the Sandbox unlock system or an explicit decision to treat "all of them" as
  the reasonable default.
- **Devotee (item #8)** — half resolved. `GhostWorldPresence.GhostID` is a small, fixed,
  compile-time roster (CC, SI, LF, SH, UW, SB — exactly the 6 real Echo regions, plus a `NoGhost`
  sentinel — all declared directly as static fields, no dynamic registration to worry about), and
  `SurvivorVanillaProgress.HasMetGhost` already exists. Missing: what "reach the Depths" actually
  corresponds to mechanically (a specific room, a region transition, some other vanilla flag) —
  needed before the "one shelter across every one of these cycles" check can be finished, since
  the final Depths cycle's own starting shelter has to match the rest.
- **Martyr (item #10)** — found `RegionState.sentientRotProgression` (`Dictionary<string,
  RotState>` with a `rotIntensity` float), gated behind `miscWorldSaveData.hasVoidWeaverAbility` —
  a real "Sentient Rot" vanilla mechanic, but tied to a specific late-game ability flag that may
  have nothing to do with an ordinary trip through Unfortunate Development. Genuinely unsure this
  is the same "rot" the challenge means by "reach a Memory Conflux room without rot" — could
  plausibly be a separate, more local hazard specific to that one room/area. Needs the user's own
  knowledge of Unfortunate Development's actual mechanics rather than a guess built on a
  same-named-but-possibly-unrelated system.
- **Guardian (item #15)** — confirmed the real class behind what the community calls "Guardians"
  is `TempleGuard` (a normal `Creature`, own `TempleGuardAI`) — but "take them both to visit the
  Guardians" reads like something more specific than mere proximity (community knowledge suggests
  TempleGuards can be appeased/passed via a specific interaction, not just walked past). Needs the
  user's own knowledge of what "meeting the Guardians" actually requires mechanically before
  picking a hook point.

20 of 50 challenges now have a tracker registered (up from 19). **Still not built or tested in a
real game** — no .NET toolchain in this container. Brace/paren balance re-checked across the whole
`src/Primal` tree after this change.

## Collector, Devotee, Martyr, Guardian implemented (2026-09-16, later still)

The user answered all 4 open questions from the previous research pass directly, unblocking every
one of them without further research needed.

**Collector.** "All means all that are collectible by either monk, survivor or hunter... select
all of the unlockables for sandbox, arena and safari that are intended for vanilla." Added
`SurvivorVanillaProgress.AllSandboxUnlockIDs()`: `ExtEnum<T>.values.entries` (confirmed public
`List<string>`) is the full registered `SandboxUnlockID` roster, reconstructed via the same
string-round-trip pattern `KillPointValue(string)` already uses. With MSC disabled — this mod's
whole target — only vanilla's own `SandboxUnlockID` registrations ever execute in the first place,
so the list comes back correctly scoped to "vanilla" with zero extra filtering code; this also
retroactively fixes a limitation `KillScores()`'s own comment flagged earlier ("this file doesn't
have a confirmed way to read [the registered count] directly" — it does, just wasn't found yet at
the time). `CollectorTracker` checks every entry via the existing `HasSandboxUnlock` at
hibernation, the same shape as `PassagesTracker`.

**Devotee.** "Reaching the Depths" = arriving in room `SB_E05`. The existing
`SurvivorVanillaProgress.HasMetGhost` reads a PERMANENT flag with no timestamp or shelter
attached — reactively checking it can't distinguish an Echo met long ago (possibly under a
completely different shelter) from one met just now as part of THIS attempt. Built a new
`SurvivorEchoEncounter.cs` instead: a Postfix on `SaveState.GhostEncounter` (confirmed via
decompilation to unconditionally set `deathPersistentSaveData.ghostsTalkedTo[ghost] = 2` right at
the top of its own body — the exact real-time moment an encounter registers), firing
`OnGhostEncountered(characterID, GhostID)` live. Gated minimally at the source (just "is this a
real character ID") since `SaveState` carries no live `RainWorld` reference to check
`PrimalSlot.IsActive` against directly — the one real consumer, `DevoteeTracker`, already re-gates
properly via `TryGetEnabledCharacter` against a live `Player`, so an ungated raise on a non-Primal
save just costs one no-op dictionary lookup there. `DevoteeTracker` locks in a "home shelter" the
first time it sees a hibernation for a character, and resets the WHOLE attempt (discarding
whatever Echoes were credited so far) the instant a hibernation happens from a DIFFERENT shelter —
matching "all from one shelter" exactly, and naturally handling the CSV's own "does not need to be
completed in consecutive successful cycles" note, since a death never touches this shelter-chain
state at all (no `OnCycleFailed` handler needed).

**Martyr.** The rot-less Memory Conflux rooms on this route are `SS_LAB7` and `SS_A01`.
`MartyrTracker`: the whole cycle must stay inside Five Pebbles (SS) from the moment the tracker
starts paying attention, touch one of those two rooms at some point, and end in a successful
hibernation — leaving SS at any point abandons the attempt outright. **Known simplifications,
flagged in the tracker's own doc comment:** doesn't verify the cycle started at the specific
"first Five Pebbles shelter" (only that it never left SS), and doesn't verify the return route
specifically passes back through Unfortunate Development (the CSV's own "must go through UD"
clause) — neither UD's own room list nor the exact starting shelter's room ID were given this
round. Worth tightening later if that data surfaces.

**Guardian.** The Guardians room is `SB_D03`. "Grapple Worm" is vanilla's own `TubeWorm` —
confirmed via decompiling `TubeWorm.cs`, which has its own `grappleToPos`/`GrappleToPosition()`/
`grapplingCounter` fields, exactly the zipline/grapple mechanic the community nickname describes
(not a guess: `TubeWorm`'s own registered display name is "Tube Worm", so this really is a
community nickname for the same creature, not a mistranslation). `GuardianTracker` tracks BOTH the
Worm and the Snail simultaneously under one `SurvivorCreatureTracking` trackerKey — that class's
existing multi-creature support (its own `ByNumber` dictionary already handles more than one
tracked creature per key) needed zero changes to do this. `Player.Malnourished` (confirmed public
bool property) is checked on every room transition while the attempt is active for the "without
starving" clause. Completes on arrival at `SB_D03` with both creatures still alive, still carried,
and no starving recorded since the attempt began. **Known simplification:** doesn't verify the
"highest room in Communication Arrays" waypoint — its room ID wasn't given this round.

24 of 50 challenges now have a tracker registered (up from 20). **Still not built or tested in a
real game** — no .NET toolchain in this container. Every vanilla member used
(`ExtEnum<T>.values.entries`, `SaveState.GhostEncounter`, `Player.Malnourished`,
`TubeWorm.grappleToPos`) was verified against `reference/GameSource/*.cs` first. Brace/paren
balance re-checked across the whole `src/Primal` tree after this change.

## Despicable implemented; 5 more mechanics researched, need user input (2026-09-16, later still)

Requested: "focus on the missing shared mechanics." Researched all 6 remaining mechanism-level
gaps from the original 19-item list — Enlightenment's neuron carry, Despicable's dream state,
Outlaw's scoring formula, Flytrap's pole plant, Triplopia's triple-spawn heuristic, Broadcast's
comms array/laser. One resolved cleanly from decompiled source alone; the other five hit real
ambiguity, each flagged explicitly below rather than guessed past.

**Despicable — implemented.** "Trigger the Stolen Enlightenment dream on the starting cycle."
Confirmed `DreamsState.DreamID.MoonThief` is "Stolen Enlightenment": traced
`RainWorldGame.Win`'s own body to the exact branch that picks it —
`GetStorySession.saveState.miscWorldSaveData.SLOracleState.neuronsLeft < 5` (the player took 5+ of
Moon's own neurons) — versus its sibling `MoonFriend`, chosen the opposite way. This is a
confirmed narrative pairing, not a name-based guess. Built a new `SurvivorDreamState.cs`.

**A genuine timing gotcha surfaced and is now documented prominently, since it's easy to get
backwards.** This is the first-ever Postfix (not Prefix) on `RainWorldGame.Win` anywhere in this
event bus. Every other Win hook (`SurvivorWinHook`, `PrimalKillCommitHook`, etc.) is a Prefix,
specifically because `Win`'s own body triggers a synchronous disk save partway through, so
anything that needs to write data before that save has to run first. Reading the dream needed the
OPPOSITE: `dreamsState.EndOfCycleProgress` (which finalizes `eventDream` for the ending cycle)
runs INSIDE `Win`'s own body, so a Prefix would read the STALE value from the previous cycle.
Confirmed via decompilation that `EndOfCycleProgress` runs before `SaveState.SessionEnded`, whose
`survived` branch calls `RainCycleTick` — the same method that increments `SaveState.cycleNumber`.
Net effect: by the time a Postfix on `Win` runs, `cycleNumber` has ALREADY moved past the cycle
that just ended. `DespicableTracker`'s own "starting cycle" check is therefore `cycleNumberAfterIncrement
== 1`, not `== 0` the way `MotherTracker`/`MycologistTracker` check it from their own
Prefix-timed reads — flagged loudly in both the event's own doc comment and the tracker's, since
this is exactly the kind of off-by-one that would silently detect the SECOND cycle instead of the
first with no error, no crash, just a subtly wrong tracker.

**The other five, each blocked on something a source-only pass couldn't settle:**

- **Enlightenment (neuron carry)** — two candidate classes exist and neither obviously fits.
  `SSOracleSwarmer` is a graspable `PhysicalObject` implementing `IPlayerEdible` (literally
  holdable like a Rock — its own `Edible` property only returns false when the player's stomach is
  already full, meaning normally picking one up just eats it). `NSHSwarmer` is a separate class
  tied to a scripted Pebbles cutscene ("green neuron," `SSOracleBehavior.cs`'s own extensive
  `SSOracleGetGreenNeuron` sub-behavior), unrelated to the general delivery mechanic. Neither
  matches "bring 7 neurons" the way this mod's other carry challenges work, since a Player can
  only hold 2 items at once — the CSV's own "Total neurons at the end must be 12 or more" note
  implies a real, PERSISTENT vanilla delivery counter (accumulated over many trips) exists
  somewhere, but no such counter turned up in a first search.
- **Outlaw (Hunter scoring)** — no real vanilla "Hunter score" field or score-screen class exists
  anywhere in the decompiled source. The CSV's own "Score calculator" external-tool link and
  "standard scorerunning rules" note confirm this is the community's OWN scoring convention that
  vanilla itself never computes or displays. This mod's existing `KillPointValue` table (already
  used by Massacre/Ranger) could sum kills across a whole run as an approximation, but per this
  doc's own earlier gap note, the real competitive formula also counts food/survival/spear-hits —
  kills alone would likely undercount against a genuine 10k threshold.
- **Flytrap (pole plant)** — found the real class: `PoleMimic` (confirmed a `Creature`, not a
  hazard object as earlier assumed), which ambushes and grabs nearby creatures via its own
  `Grab(...)` call. Unclear: the actual player technique for getting a live Vulture into its grab
  range (does it just need to wander close, or is there a specific setup), and what distinguishes
  "successfully fed, player survived" from any other grab the plant might make.
- **Triplopia (triple-spawn heuristic)** — a known community EXPLOIT technique (the CSV's own
  external "Multi-spawning documentation" link), not a normal vanilla mechanic. This doc already
  called it "inherently fragile" before this pass; no save-data signature to key off turned up
  either. Leaning toward this staying honor-system rather than forcing a heuristic.
- **Broadcast (Communications Array signal)** — nothing found at all. No "laser," "signal," or
  "array" string anywhere in the decompiled source, and no dedicated room-effect type the way
  `ElectricDeath` was findable for Room's own hazard system. Genuinely stuck on where this
  mechanic even lives in vanilla's own code — this may need the user's own knowledge just to know
  where to look.

25 of 50 challenges now have a tracker registered (up from 24). **Still not built or tested in a
real game** — no .NET toolchain in this container. Brace/paren balance re-checked across the whole
`src/Primal` tree after this change.

## Enlightenment and Flytrap implemented; Triplopia deferred; Outlaw/Broadcast still stuck (2026-09-16, later still)

The user answered all 4 remaining questions from the previous research pass. Two unblocked
cleanly, one is explicitly deferred by the user's own direction, one is still genuinely
unresolved.

**Enlightenment — implemented.** "SL_AI contains the Looks to the Moon iterator entity and if you
throw a neuron to her, she stores them." Traced this to `SLOracleBehavior.ConvertingSSSwarmer()`,
which increments `SLOrcacleState.neuronsLeft` — confirmed to be the SAME persistent field
`DespicableTracker`'s own `MoonThief` detection already reads (`neuronsLeft < 5` triggers that
dream), proving this is one real, continuously-updating vanilla counter rather than something this
mod would need to invent. `EnlightenmentTracker` checks a plain `neuronsLeft >= 12` threshold,
matching the CSV's own explicit numeric requirement ("Total neurons at the end of the challenge
must be 12 or more") directly, rather than trying to derive "+7 from wherever the player started"
from the vaguer "bring 7" phrasing in the main description.

"Without passaging" needed a genuinely permanent, save-file-wide disqualification — a passage used
even once, ever, on this character's save disqualifies it forever, not just within one narrow
attempt. This required extending `SurvivorEvents.OnPassageUsed`'s own signature to also hand back
`DeathPersistentSaveData` (previously just `(string characterID, WinState.EndgameID endgameID)`)
so a permanent `PrimalSaveMarkers` flag could be written the instant a passage fires. Safe to
extend now specifically because nothing else consumed that event yet (confirmed via grep before
changing it) — extending an event's signature after a second real consumer exists would be a
different, riskier call.

**Flytrap — implemented.** "Anger the pole mimic so it is in its active state and then have a
vulture go over it — when the pole mimic tries to latch onto anything passing by, it grabs the
vulture and drags it into its den." Traced this to `PoleMimic.Collide(PhysicalObject, int, int)`,
which contains exactly this condition gating its own ambush grab: `base.Consious && WakeUp >
0.3f && ... Grab(otherObject, 0, otherChunk, ...)` — `Consious`/`WakeUp > 0.3f` is the real state
behind "angered/active" the user described. Built a new `SurvivorPoleMimicPredation.cs`: a Postfix
on `Collide` checking `otherObject is Vulture` and confirming the grab actually landed
(`grasps[0]?.grabbed == vulture` after the call, since several other conditions inside `Collide`
can prevent the grab from happening at all). **Known simplification, flagged in both the event's
own doc comment and `FlytrapTracker`'s:** no explicit prey-KILLING method was found on `PoleMimic`
itself (unlike `BigEel.Swallow`'s own clean, explicit kill call) — this treats the GRAB itself as
"fed" rather than waiting to confirm an actual death, and reads "survive the encounter" as "the
player is not currently dead at the moment the grab lands," the same live-framing
`OfferingTracker`'s own "survive" clause already uses. Since the event fires off an autonomous
creature-vs-creature collision with no player directly involved, `FlytrapTracker` resolves a
credited player by iterating the game's own `Players` list — the same pattern
`PerseveranceTracker`'s own `OnGameEnding` consumption already establishes for "no player handed
directly, but there's a live game session to search."

**Triplopia — deferred, per the user's own explicit direction:** "there may not be a proper
technical signature — this may just be a bug that places multiple creatures into dens, which
would be very difficult to detect reliably. Put this away for now as one of the last trackers to
implement." Not built. Revisit later, last among the remaining gaps.

**Outlaw — still unresolved.** The user described a real, more complete formula: deaths and quits
subtract, food eaten and survived cycles add, multiplied by total passages acquired — present in
Hunter's own "ran out of cycles, died at cycle 0" ending specifically. Traced
`WinState.CycleCompleted`, the method actually behind the REAL, in-game `WinState.EndgameID
.Outlaw` passage token (confirmed this is a genuine vanilla passage, not just a community
challenge name that happens to match) — but its own Outlaw-tracker contribution
(`num2`, added to `IntegerTracker.progress`) turns out to be nothing more than a per-cycle count
of kills where `CreatureTemplate.countsAsAKill > 1` — nowhere near the richer formula described.
Searched `RedsIllness.cs` (Hunter's own cycle-limit/lifespan mechanic) and broadly for
"quitCount"/"survivalScore"/similar terms across the whole decompiled tree with no match at all.
Two possibilities remain open: either the real formula lives somewhere this pass's search terms
simply missed, or (matching the CSV's own "Score calculator" external-tool reference) it's
genuinely a community-only calculation performed from raw end-of-run stats rather than a single
number vanilla itself ever computes.

27 of 50 challenges now have a tracker registered (up from 25). **Still not built or tested in a
real game** — no .NET toolchain in this container. Every vanilla member used
(`SLOracleBehavior.ConvertingSSSwarmer`, `SLOrcacleState.neuronsLeft`,
`PoleMimic.Collide`/`Consious`/`WakeUp`) was verified against `reference/GameSource/*.cs` first.
Brace/paren balance re-checked across the whole `src/Primal` tree after this change.

## Manslaughter, Chieftain, Criminal wired up (2026-09-16, later still)

Requested: "put Outlaw aside next to Triplopia, continue implementing next features." With both
of those explicitly parked, moved on to challenges whose underlying MECHANISMS already existed
from earlier batches but had zero consumers yet — `SurvivorTollState`/`SurvivorTollRoster` (toll
state) and `SurvivorCreatureTracking` (identity tracking). No new shared mechanisms needed this
round, just wiring.

**ManslaughterTracker** — the first real consumer of `SurvivorTollRoster`, built purely for this
challenge back when the toll mechanism was extended but never used. All three of its own
conditions ("every guard here ever died," "zero player kill credit," "zero hostility ever")
were already directly answerable by that file's own `AllEverSeenGuardsDead`/
`PlayerEverCreditedAGuardKill` — the only new piece needed at the tracker level was a small local
`(characterID, roomKey) -> ever hostile` set, fed by the already-existing
`SurvivorTollState.OnOutpostTransgression`. No cycle-scoping anywhere in the challenge's own CSV
wording, which matches `SurvivorTollRoster`'s own design — it was already built to persist across
however many cycles it takes, not reset per attempt.

**ChieftainTracker** — "Bring live Scavengers to the Looks to the Moon subregion." The destination
room fell straight out of the PREVIOUS batch's own Enlightenment research: `SL_AI` is the room
Moon's own Oracle behavior (`SLOracleBehavior.ConvertingSSSwarmer`) runs in, and Shoreline (SL) is
exactly the region the challenge's own verification note references ("Scavengers at the Karma
Gate into Shoreline + the same Scavengers in Looks to the Moon"). The real design point: Chieftain
is genuinely different from every other creature-based carry challenge in this mod so far —
Scavengers here are TAMED ALLIES that follow the player on their own, never grabbed via
`Player.grasps`, so this is the first tracker to use `SurvivorCreatureTracking`'s own identity
layer (`Touch`/`AllTrackedAlive`/`FindInRoom`) WITHOUT `IsBeingCarriedBy` at all — confirming that
helper's earlier design note (from when `IsBeingCarriedBy` was first added) that Chieftain/
Pilgrimage's own "escort, not carry" cases would need no carry check whatsoever was correct.
**Known simplification, flagged in the tracker's own doc comment:** requires 2+ live Scavengers
together in Shoreline as the "brought" set — the challenge's own text only ever says "Scavengers"
(plural), with no explicit count anywhere in its CSV row, so 2 is a judgment call for the smallest
number satisfying that plural.

**CriminalTracker** — "Travel to the Garbage Wastes Stronghold after passing three Scavenger
Tolls without paying on the starting cycle." A direct, simple consumer of
`SurvivorTollState.OnTollPassed` (`paid == false`), deduplicated by `SurvivorTollRoster
.RoomKeyFor` so re-passing the same toll twice doesn't double-count. Cycle-0 checked during
ordinary `Player.NewRoom`-driven room-entry, not a `RainWorldGame.Win` Prefix/Postfix — worth
noting explicitly that NONE of `DespicableTracker`'s own cycleNumber-timing gotcha applies here,
since `SaveState.cycleNumber` is simply stable for the whole cycle once set and this reads it
mid-cycle, not around the Win boundary. **Known simplification:** doesn't verify arrival at the
actual "Garbage Wastes Stronghold" room (its exact room ID wasn't available this pass) — completes
as soon as the 3rd distinct unpaid toll is passed while the attempt has stayed inside Garbage
Wastes the whole time.

**Left unwired on purpose, not forgotten:** Heist (needs identifying the exact "lowest" GW toll
among potentially several outposts in that region — more precise than "any" toll) and Elder (needs
the still-entirely-unbuilt "Scavenger wearing a Vulture Mask as an item" detection, a genuinely
different mechanic from the live-Vulture-demasking event already built for Usurper). Both would
need real new data or research, not just wiring, so they stayed out of this batch rather than
shipping a much weaker approximation.

30 of 50 challenges now have a tracker registered (up from 27). **Still not built or tested in a
real game** — no .NET toolchain in this container. Brace/paren balance re-checked across the whole
`src/Primal` tree after this change.

## Nomad, Circuit, Desperado, U-Turn, Symbiote, Charged wired up (2026-09-16, later still)

Requested: "let's do the room-named-blocked and carry challenges — what do you need from me." The
user supplied exact room IDs for 6 of the 9 previously-blocked challenges directly, explicitly
parking Journey ("as a hard task"), Visitor ("as a hard task"), and Fishtank ("as a hard task") for
a later pass. No new shared mechanisms needed for 5 of these 6 — pure wiring against rooms this
mod had no way to know before. Desperado needed one genuinely new piece.

**NomadTracker** — the 4 Industrial Complex Karma Gates: `GATE_HI_GW`, `GATE_SU_HI`, `GATE_HI_CC`,
`GATE_HI_SH`. Completes the instant the 4th distinct gate room is entered — no hibernation
required at all, matching the CSV's own explicit "You do not need to hibernate!" note. **Known
simplification:** treats "entered the gate room" as "crossed the gate" (the challenge's own
wording is more precise — "crossing the middle line" — but this mod has no in-room position data,
only room identity).

**CircuitTracker** — start/end shelter `SS_S05`. The 3 checkpoint rooms (`SS_D04`, `SS_I03`,
`SS_A03`) were already sitting in this doc's own Console Verification column from the very
original CSV pull, just never connected to code before. This is the first real consumer of
`SurvivorCheckpointRun` — built during the ORIGINAL 8-mechanism shared-mechanics batch, purpose-
built for exactly this challenge shape, and sitting completely unused until this pass. 3m45s
converts to 9000 ticks at the confirmed 40-ticks/second rate.

**DesperadoTracker** — "Deer Pearl room" is `LF_D07`. This one needed real new research, not just
a room ID: confirmed "Popcorn Plant" is the community nickname for vanilla's own `SeedCob`
(`MoreSlugcats/SlugNPCAI.cs`'s own `GetFoodType` method explicitly categorizes a `SeedCob` as
`Food.Popcorn`) — but `SeedCob` does NOT implement `IPlayerEdible` and is never eaten through
`Player.ObjectEaten` at all (confirmed via decompiling `SeedCob.cs`, which has no such interface).
Its own `Update` body instead calls `PlayerSessionRecord.AddEat(this)` directly once a nearby,
hungry, eligible player stands close enough for long enough (guarded by
`player.eatExternalFoodSourceCounter` so it only fires once per real eating session, not once per
frame of contact). Built a new file, `SurvivorSeedCobEaten.cs`, patching
`PlayerSessionRecord.AddEat` directly and resolving the eating player via
`PlayerSessionRecord.playerNumber` (confirmed public, the same field `AddEat`'s own body already
uses internally) — the real, universal "ate an external food source" signal this object uses
instead of the path `SurvivorEvents.OnObjectEaten` already covers. Tracks 2 distinct `SeedCob`
instances eaten while inside `LF_D07`; "escape alive" reads as "successfully transitioned to a
different room" (any `Player.NewRoom` firing implies the player is alive and moving).

**UTurnTracker** — northeast Outskirts shelter is `SU_S04`. Same shape as Circuit but with ZERO
named checkpoints — `SurvivorCheckpointRun.AllCheckpointsHit` (`_nextCheckpoint >=
_checkpointRoomsInOrder.Length`) is vacuously true for an empty array, so this needed no
special-casing at all: it's exactly the "no named checkpoints, just a timed loop" case that
class's own ORIGINAL header comment already anticipated when it was first built, now finally
exercised. 1 minute converts to 2400 ticks, the same constant this codebase already uses
elsewhere.

**SymbioteTracker** — westernmost Underhang shelter `UW_S01`, shelter at the top of The Leg
`UW_S07`. Confirmed "squidcada" is the community nickname for vanilla's Cicada
(`CreatureTemplate.Type.CicadaA`/`CicadaB`) — both variants accepted, since the challenge's own
text doesn't specify which. "While starving" (`Player.Malnourished`) is checked CONTINUOUSLY at
every room transition during the carry, not just once at the start — becoming well-fed at any
point during the journey abandons the attempt, matching the literal framing rather than a looser
"was starving at some point" reading.

**ChargedTracker** — eastmost Drainage System shelter `DS_S02`. "Baby centipede" is
`CreatureTemplate.Type.SmallCentipede` specifically (the "youth" size class, distinct from plain
`Centipede` and `RedCentipede` per Ranger's own CSV note that these three count as distinct
species). Completion is checked at the moment of hibernation by verifying the SPECIFIC tracked
instance is present in `SurvivorShelterContents.EntitiesInShelter` — not by re-checking
`IsBeingCarriedBy` at that exact instant, which would be less robust against however grasp state
behaves during the sleep transition. Same reasoning `MotherTracker`/`SleepoverTracker`'s own
shelter-presence checks already use.

36 of 50 challenges now have a tracker registered (up from 30). **Deferred, per the user's own
explicit direction, as hard tasks for a later pass:** Journey, Visitor, Fishtank. **Still not built
or tested in a real game** — no .NET toolchain in this container. Every vanilla member used
(`SlugNPCAI.GetFoodType`, `SeedCob`, `PlayerSessionRecord.AddEat`/`.playerNumber`,
`CreatureTemplate.Type.SmallCentipede`/`CicadaA`/`CicadaB`) was verified against
`reference/GameSource/*.cs` first. Brace/paren balance re-checked across the whole `src/Primal`
tree after this change.

## The 6-challenge accounting gap: Catalogue and Ascension implemented (2026-09-16, later still)

The user caught a real bookkeeping gap: 36 implemented + 8 explicitly flagged as difficult
(Journey, Visitor, Fishtank, Outlaw, Triplopia, Broadcast, Heist, Elder) only accounts for 44 of
50 — Catalogue, Vegan, Friend, Scholar, Saint, and Ascension had simply fallen through the cracks
since the original ratings pass and were never revisited. Went through all 6 properly.

**Catalogue — implemented.** "Place 15 unique objects/creatures into a shelter in one cycle." A
direct, overdue consumer of `SurvivorShelterContents.CountDistinct` — shared mechanism #2, built
in the very first shared-mechanisms batch, and (unlike its sibling `CountObjectsOfType`, already
used by `MotherTracker`) never actually consumed until now. Dedupes by a combined
creature-or-item type key (`"C:" + creatureType`/`"I:" + itemType`, so the two enum namespaces
can never collide). **Known simplification:** the challenge's own rule is "every unique ICON is a
unique object" — this dedupes by raw type instead, since this mod has no icon-grouping data to
check against; could diverge if vanilla's own icon system groups some distinct types under one
shared icon, or vice versa.

**Ascension — implemented.** Reuses the exact same `standardVoidSea` ending flag
`PerseveranceTracker` already consumes — reaching this ending at all already implies having
traveled through Five Pebbles, so (matching Perseverance's own reasoning for "reaching Depths")
no separate waypoint check was added. Confirmed via decompilation that `RainWorldGame.clock` (a
plain public int, incremented exactly once per `RawUpdate` call) has NO reset call anywhere in
either `RainWorldGame.cs` or `SaveState.cs` — it accumulates for the entire game session from
creation, which is exactly "IGT from game start," no separate start-time bookkeeping needed at
all. 38 minutes converts to 91200 ticks at the confirmed 40-ticks/second rate. **Known
simplification:** the CSV's own notes say completion is really measured by "the Remix speedrun
timer's value when it stops" — this reads the mod's own raw `game.clock` instead, which could
diverge from that real timer (e.g. if the Remix timer excludes paused time this reading doesn't).
"No skipping Guardians" is left as an honor-system note, matching Perseverance's own treatment of
its "no Guardian skip" clause.

**The other 4 — re-examined and correctly re-categorized, not silently dropped a second time:**

- **Vegan** genuinely belongs alongside Visitor in the "hard task" bucket — both need exact
  room/position data inside the tutorial area (Vegan's 7 fruit locations plus the charge-hop gap
  threshold) that hasn't been supplied. Not a new category of gap, just one that got missed when
  Visitor was called out but Vegan wasn't.
- **Friend** is a genuinely distinct, real architectural gap correctly identified back in the
  original ratings pass but never followed up on since: the TAMING check itself is already solved
  (the same `friendTracker.friend` expression `SleepoverTracker` already trusts), but the
  challenge needs the RESULT (which Lizard types have been tamed) tracked as a set SHARED ACROSS
  EVERY PRIMAL CHARACTER on the save slot — "Friends can be tamed in any amount of campaigns" —
  not per-character the way every other persistent marker in this mod
  (`PrimalSaveMarkers`/`DeathPersistentSaveData`) is scoped. The only existing cross-character
  persistence layer in this mod is the Remix-config-backed `Configurable<T>` mechanism
  `GimmickAssignments` already uses for gimmick-to-character assignments — reusing that pattern
  for a hidden, non-UI "which Lizard types has ANY character ever tamed" bitmask seems plausible,
  but wasn't investigated deeply enough this pass (in particular, whether a `Configurable<T>`
  persists correctly without ever being attached to an actual on-screen UI element, only bound
  programmatically) to build with real confidence.
- **Scholar** — the enumeration mechanism is now trivial: `DataPearl.AbstractDataPearl
  .DataPearlType` is confirmed to also be an ExtEnum, so the exact same `values.entries` trick
  Collector uses applies with zero new research. The real open question is SCOPE, and it's
  sharper here than Collector's own version of the same question: some registered pearl colors
  are tied to unique, one-off STORY pearls that may never spawn as a loose collectible at all, so
  blindly treating "every registered DataPearlType" as the completion threshold (Collector's own
  approach) risks shipping a tracker that can mathematically never complete. Needs the same kind
  of scope confirmation Collector got before this gets built the same way.
- **Saint** — re-confirmed as genuinely underspecified, not newly discovered: "complete all 3
  objectives without killing anything, or eating live prey," with exclusion clauses ("no karma
  caching," "no echo-negated kills") the very first ratings pass already flagged as likely
  "partially honor-system by necessity, not just by choice." Nothing new turned up to change that
  assessment.

38 of 50 challenges now have a tracker registered (up from 36). **Still not built or tested in a
real game** — no .NET toolchain in this container. Brace/paren balance re-checked across the whole
`src/Primal` tree after this change.

## Friend and Scholar implemented (2026-09-16, later still)

The user resolved both remaining open questions directly, and both turned out simpler than the
previous pass assumed — worth internalizing as a lesson: the original ratings pass's own
architectural assumption for Friend was wrong, not just incomplete.

**Friend.** "Story mode refers to game mode and we have only been in story mode — basically just
track if all lizard types got tamed in one run of one character." This is PER-CHARACTER, this
mod's default scope — the earlier assumption that it needed tracking SHARED ACROSS EVERY PRIMAL
CHARACTER on a save slot was simply incorrect, not a real architectural gap needing new
cross-character persistence infrastructure. What genuinely WAS needed: a LIVE "just got tamed"
signal. The existing `friendTracker.friend` field (already trusted directly elsewhere in this mod
— `ChainsGimmick.cs`, `SleepoverTracker`) only ever shows the MOST RECENT tame at any given
moment; a permanent per-species record needs to know the exact INSTANT a new taming happens, not
just what's currently true (a previously-tamed Lizard could die or wander off, and a reactive
check would lose that historical fact). Traced this to `FriendTracker.Update()`: confirmed via
decompilation that `friend` transitions from null to a real `Creature` exactly when a relationship
crosses `like > 0.5f && tempLike > 0.5f`. Built a new `SurvivorTaming.cs`, using the same
wasNull/isNotNull transition trick as every other "X just happened" detection in this event bus.

**Naming collision, avoided on purpose:** the challenge tracker itself is named
`FriendChallengeTracker`, NOT `FriendTracker` — vanilla already has a real `FriendTracker` class
(the one `SurvivorTaming.cs` itself patches), and a second type with that exact bare name inside
the same `Primal.Gimmicks` namespace would shadow the vanilla one for any file in this namespace
that references it unqualified, silently breaking `[HarmonyPatch(typeof(FriendTracker), ...)]`
wherever it's used. Every OTHER challenge tracker in this codebase follows the plain
`<ChallengeName>Tracker` convention; this is the one deliberate exception, and the reason is
recorded here so a future pass doesn't rename it back for consistency without realizing why it's
different.

**Scholar.** "Do not worry about uncompletable challenges... for all colors, use all colors
accessible in vanilla, for survivor, monk and hunter." Confirmed
`DataPearl.AbstractDataPearl.DataPearlType` is also an ExtEnum (nested two levels, matching the
same syntax already used to reference it in `DespicableTracker`'s general research), so the exact
same `values.entries` trick already proven for `SandboxUnlockID` applies directly — added
`SurvivorVanillaProgress.AllDataPearlTypes()`, structurally identical to `AllSandboxUnlockIDs()`.
`ScholarTracker` itself is a direct consumer of `SurvivorShelterContents.CountDistinct`, keyed on
`AbstractDataPearl.dataPearlType` (the pearl's own color/identity field, confirmed via
decompilation to be distinct from the generic `AbstractObjectType` every pearl instance shares —
`type` is always `AbstractObjectType.DataPearl`, `dataPearlType` is the actual color). Per the
user's own explicit instruction, no filtering was added for registered colors that may belong to
one-off story pearls incapable of ever appearing as a loose collectible — accepted the same way
several other challenges (the ones scoped to "the starting cycle") are already understood to
become permanently uncompletable once that window passes, without that being treated as a defect
needing a workaround.

40 of 50 challenges now have a tracker registered (up from 38). **Still open, all previously
discussed and none newly discovered:** Journey, Visitor, Vegan, Fishtank, Outlaw, Triplopia,
Broadcast, Heist, Elder, Saint. **Still not built or tested in a real game** — no .NET toolchain
in this container. Every vanilla member used (`FriendTracker.Update`/`.friend`,
`DataPearl.AbstractDataPearl.DataPearlType`, `AbstractDataPearl.dataPearlType`) was verified
against `reference/GameSource/*.cs` first. Brace/paren balance re-checked across the whole
`src/Primal` tree after this change.

## Final status before merge to main (2026-09-16): the remaining 10 challenges, precisely scoped

`survivor-gimmick` merges to `main` with 40 of 50 community challenges tracked. This section is
the single source of truth for what's left — read this instead of re-deriving it from the
per-challenge history above. Each of the 10 has exactly one blocking gap, not a vague "needs more
work":

1. **Journey** (reach the Depths from the highest Five Pebbles shelter in one cycle) — needs one
   room ID (the starting shelter; the Depths-entry destination `SB_E05` is already known from
   Devotee). Mechanically identical to `UTurnTracker` once supplied.
2. **Visitor** (leave the tutorial, re-enter, return to the exact first-spawn platform before rain)
   — needs the confirmed tutorial spawn room/position. A candidate room, `SU_A22`, was floated from
   `Menu/RegionSelectMenu.cs`'s debug start-room list but never verified.
3. **Vegan** (eat all 7 tutorial-area blue fruit before the first charge-hop gap crossing) — needs
   the same tutorial-area geometry as Visitor, plus the 7 fruit positions and the gap-room ID.
4. **Fishtank** (7 Hazers from unique spawns, alive, into one body of water) — needs a Hazer den
   map so "unique spawn location" can be verified; the CSV references an external community map.
   Counting 7 live Hazers moved together is otherwise already within this mod's existing
   carry-tracking primitives.
5. **Outlaw** (10k+ Hunter-run score) — needs the user to point at the real scoring formula's
   location in source (described as: deaths/quits subtract, food/survival add, multiplied by total
   passages; said to appear in the Hunter cycle-0/out-of-cycles ending). Searched
   `WinState.CycleCompleted` (found `Outlaw` is a real passage token, but its own contribution is
   an unrelated narrow kill-count) and `RedsIllness.cs` with no match — may live in a DLC assembly
   not currently decompiled into `reference/GameSource`.
6. **Triplopia** (triple-spawn Chimney Canopy, survive the starve cycle) — user already confirmed
   this is likely a signature-less emergent bug. Needs either a concrete observable side effect to
   key on, or a decision to leave it untracked.
7. **Broadcast** (Sky Islands laser-tower traversal ending in a scripted Vulture death) — zero
   technical foothold found after exhaustively searching "laser," "signal," "array," "beam,"
   "relay," "transmission," "antenna" across the whole decompiled source. The only one of the 10
   with no lead at all. Needs the user's own knowledge of the real class/field names.
8. **Heist** (steal from, don't pay, the *lowest* GW Scavenger Toll, then hibernate with the pearl
   before rain) — the toll mechanism itself (`SurvivorTollState.OnItemStolen`/`HasPaidToll`) is
   already built and proven by Manslaughter/Chieftain/Criminal. Needs only the specific room ID for
   "the lowest" toll, or an explicit decision to accept "any GW toll" as a simplification.
9. **Elder** (mask 6 Scavengers with Vulture Masks at any toll in one cycle) — needs a wholly new
   mechanism: detecting a Scavenger's *equipped* mask item, which is unrelated to the existing
   live-Vulture-demasking event (`SurvivorEvents.OnNonLethalDemask`) built for Usurper. No research
   done yet into `Scavenger.cs`/`ScavengerAI.cs`'s item-equip representation. Also needs a scope
   call from the user: 6 simultaneously-masked Scavengers alive at once, vs. 6 masking events over
   the cycle (these diverge if a masked Scavenger dies mid-cycle and gets replaced).
10. **Saint** (finish all 3 objectives with 0 kills / 0 live-prey-eaten, excluding karma-caching,
    echo-negated kills, and Guardian Skip) — re-confirmed genuinely underspecified, not newly
    found: the exclusion clauses describe technique legality, which isn't observable game state.
    The trackable core (kill count + live-prey-eaten count across a full run) is buildable today
    from `PrimalKillTracker` and existing eaten-food tracking. Needs a decision: ship that core as
    an honor-system approximation of the full rule, or leave unimplemented.

**None of the 40 implemented trackers have been built or tested in a real game** — no .NET
toolchain was available in this container for this entire session. Treat all 40 as "should compile
and behave correctly per source-level review, verified against `reference/GameSource/*.cs`," not as
confirmed-working.

## 2026-09-17: 4 more of the 10 remaining challenges implemented — 44 of 50 now tracked

Picked up directly from "Final status before merge to main" above, on branch
`claude/peaceful-babbage-tfadzw` (46 commits ahead of `main` as of session start — the
`survivor-gimmick` merge described above hadn't actually reached `origin/main` yet; `main` was
still just its initial commit). Reviewed all 10 remaining challenges again from scratch rather than
trusting last session's own conclusions unchecked, and found two of the earlier "blocked" verdicts
were wrong, not just incomplete:

- **Elder — implemented.** The earlier doc entry said this "needs a wholly new mechanism: detecting
  a Scavenger's *equipped* mask item." Re-reading the CSV's own evidence requirement ("a
  screenshot/short clip of 6 Scavengers **holding** the Masks at a toll") shows this was never about
  equipping anything — vanilla has no mask-wearing slot for an ordinary Scavenger at all, only a
  grasp, the same thing `ScavengerAI`'s own item-desirability table already scores `VultureMask`
  for. New shared mechanism added for this: `SurvivorTollState.OnOutpostTick`, a per-tick event
  piggybacked onto `SurvivorTollRosterObserveHook`'s existing Postfix on `ScavengerOutpost.Update`
  (no second Harmony patch needed on the same method). `ElderTracker` (`ChallengeTrackers.cs`)
  checks every tick whether 6+ distinct live Scavengers in an active outpost's own room
  (`Room.updateList`) are each currently grasping a `VultureMask`. Stateless — a plain snapshot
  check needs no cycle-scoping, since any single tick is inherently within one cycle.
- **Heist — implemented, with a flagged simplification.** "The **lowest** Garbage Wastes Scavenger
  Toll" still has no known room ID (same gap `CriminalTracker`'s own "Stronghold" already accepted a
  simplification for) — this accepts ANY Garbage Wastes toll instead, same treatment. Everything
  else is real: `SurvivorTollState.OnItemStolen` (already-existing mechanism, previously unused by
  any tracker) filtered to a `DataPearl` stolen from a `GW_`-prefixed room, correlated by the exact
  stolen pearl's own `EntityID.number` (not just "some pearl") against
  `SurvivorShelterContents.EntitiesInShelter` at the next `OnSuccessfulHibernation` — the same
  specific-instance correlation `FeastTracker`/`UsurperTracker` already established.
- **Triplopia — implemented, with a flagged simplification.** The earlier doc entry said this was
  "likely a signature-less emergent bug" needing "a concrete observable side effect to key on, or a
  decision to leave it untracked." The CSV's own secondary evidence line supplies exactly that
  signature: "short clip of killing **3 Pole Plants spawned in the same den**." A triple-spawned
  region manifests as its own normally-unique objects (like a den's one Pole Plant,
  `CreatureTemplate.Type.PoleMimic`) existing 3 times at once. `TriplopiaTracker` completes the
  instant 3+ live `PoleMimic`s are seen simultaneously in one Chimney Canopy (`CC`) room. Flagged
  gap: doesn't check "survive the starve cycle" (no malnourishment/forced-cycle state), and doesn't
  require actually killing them or that the sighting/kill happen on separate cycles the way the
  CSV's own two-part verification implies — same class of trade-off as Nomad's "entered the gate
  room" standing in for "crossed the middle line."
- **Outlaw — implemented.** The earlier doc entry said the real Hunter-run scoring formula had been
  searched for exhaustively with no result, and might live in an undecompiled DLC assembly. It
  doesn't — decompiling `Menu/StoryGameStatisticsScreen.cs` (the CSV's own "Screenshot of final
  score screen" hint, taken literally) turned up the whole thing:
  `StoryGameStatisticsScreen.ScoreKeeper` computes `sum(food×1 + survivedCycles×10 + deaths×-3 +
  quits×-3 + (totalSeconds/60)×-1 + perKillCreatureValue)` (creature values from vanilla's own
  `GetNonSandboxKillscore`/`killScores` table) then **multiplies the whole thing by (1 + however
  many endgame passages are already fulfilled)** — confirming the CSV's own "follow other standard
  scorerunning rules" hint. Rather than re-deriving that formula (the kill-value table alone is
  large), `OutlawTracker` reads vanilla's own already-computed answer straight off
  `ScoreKeeper.score` once the screen's own animation finishes (`StoryGameStatisticsScreen.
  allDone`), the same "ride vanilla's own tested machinery" preference used throughout this mod.
  This is the **first ChallengeTracker with no live Player** to detect against — by the time this
  screen exists the story session is fully over — which needed two small new pieces of shared
  plumbing rather than anything Outlaw-specific: `ChallengeCompletion.ReportCompletionForSave`
  (persists a completion straight from a `DeathPersistentSaveData` reference, no `Player`/
  `RainWorldGame` needed, and never fires the live popup since there's no HUD on this screen to show
  it on) and three new reflected accessors on `PrimalMenuAccess`
  (`IsScoreScreenDone`/`GetFinalScore`, alongside the existing `GetSaveState`/`GetCharacterID` which
  already covered this exact screen family via its `KarmaLadderScreen` base class) — reflected
  rather than direct field access on the same Gotcha #1 reasoning `PrimalMenuAccess`'s own header
  comment already gives for `saveState`, even though nothing here has actually been caught broken
  yet, since this is genuinely new territory for the mod. Detected via its own Harmony Postfix on
  `StoryGameStatisticsScreen.Update` (`OutlawScoreScreenHook`) rather than a `SurvivorEvents`
  subscription, since there's no gameplay-side event this challenge's own detection point could
  hang off.

**2026-09-17, same day, user feedback on the 4 above — two corrections applied, two confirmed
as-is:**
- **Elder — confirmed correct as implemented** (Scavengers just hold masks, no equip mechanic).
- **Heist — the user supplied the exact room ID: the lowest Garbage Wastes Scavenger Toll is
  `GW_C11`.** `HeistTracker` now checks that exact room instead of "any `GW_`-prefixed toll" — no
  simplification needed anymore. **Going forward: when a challenge's own CSV text names a specific
  room ("the lowest," "the highest," "the northeast," etc.) and this repo has no way to determine
  which real room that is, ask the user for the room ID directly rather than defaulting to a
  same-region simplification** — this is exactly the kind of fact only the user's own in-game
  knowledge can supply, the same way every other room ID this mod relies on (Devotee's `SB_E05`,
  Martyr's `SS_LAB7`/`SS_A01`, Nomad's 4 gate rooms, Chieftain's `SL_AI`) was originally obtained.
- **Triplopia — the completion condition was wrong, not just simplified.** The user confirmed the
  triple-spawn/den-identification approach but corrected what actually completes it: not "3 live
  PoleMimics observed together" (my original implementation), but **killing** 3 PoleMimics that all
  come from the same den. Rewritten: `TriplopiaPoleMimicDeathHook`, a new Postfix on `Creature.Die`
  scoped to `PoleMimic` in Chimney Canopy, resolves killer attribution the same way
  `SurvivorTollRosterDeathHook` already does (`Creature.killTag` falling back to
  `PrimalKillTracker.TryGetRecentHandler` — duplicated rather than depended-on-in-order against
  `PrimalKillDieHook`'s own independent Postfix on the same method), keys each kill by
  `(characterID, roomName + abstractNode)` — `abstractNode` is the specific den node a creature's
  `WorldCoordinate` resolves to, so 3 triple-spawned copies of one den's occupant all share the same
  key while 3 incidental unrelated PoleMimics would not — and completes once a den's own kill count
  reaches 3.
- **Outlaw — confirmed correct as implemented** ("on game end, track to see if it succeeded" is
  exactly what `OutlawScoreScreenHook` does).

**Broadcast and the other 6 — re-checked, genuinely still blocked, not just untried again:**
- **Broadcast** — repeated the same broad keyword search ("laser," "beam," "signal," "array,"
  "relay," "transmission," "antenna," "Communication," "Sky Island") across the whole decompiled
  source fresh this session; still zero hits beyond menu/scene-illustration code. Confirmed, not
  just repeated: whatever implements the Communications Array laser-tower mechanic isn't present
  anywhere in `reference/GameSource/` at all (it may be encoded in room/level data this repo simply
  doesn't have, not missed source scanning). Still needs the user's own knowledge of the real
  class/field names, if any exist as identifiable C# state at all rather than being purely
  level-geometry-driven.
- **Journey, Visitor, Vegan, Fishtank** — all four need exact room IDs or community map data (a
  specific shelter, the tutorial's exact spawn room/fruit positions, a Hazer den map) that simply
  isn't derivable from decompiled C# source — this repo has no level/room geometry files at all,
  only compiled behavior. Every room ID this mod's own trackers already rely on elsewhere (Devotee's
  `SB_E05`, Martyr's `SS_LAB7`/`SS_A01`, Nomad's four gate rooms, Chieftain's `SL_AI`) was supplied
  by the user directly, not found by searching source — these four are waiting on the same kind of
  input, not a deeper search pass.
- **Saint** — unchanged from the prior write-up: genuinely underspecified (the exclusion clauses
  describe technique legality, not observable state), not a research gap.

**None of these 4 new trackers have been built or tested in-game** — same standing caveat as the
other 40 (no `.NET` toolchain in this container this session either). `OutlawTracker` in particular
touches an entirely new class family (`Menu.StoryGameStatisticsScreen`/`KarmaLadderScreen.
ScoreKeeper`) this mod has never compiled against before, so treat it with a bit more suspicion than
the other three (which all reuse already-proven mechanisms) until it's actually been through a
build.

## 2026-09-17 (later): Journey, Visitor, Fishtank implemented off user-supplied room IDs — 47 of 50

The user supplied the exact room IDs this doc's own "Broadcast and the other 6" section above said
were unreachable from source alone, plus one clarification each on the actual completion shape:

- **Journey — "Reach the Depths from the highest shelter in Five Pebbles in one cycle."** Highest
  shelter: `SS_S04`. Depths entry: `SB_E05` (already established by DevoteeTracker). **Correction to
  this doc's own earlier guess:** the "Final status" section above said this would be "mechanically
  identical to UTurnTracker" — it isn't. UTurnTracker is a round-trip-within-a-time-limit shape
  (`SurvivorCheckpointRun`); Journey is one-way with no time limit, so it's actually much closer to
  DevoteeTracker's own HomeShelter/DepthsReached shape: hibernating at `SS_S04` arms the *next*
  cycle's watch for `SB_E05`, and hibernating anywhere else disarms it. Persists across cycles
  ("on any cycle" — no per-cycle reset needed).
- **Visitor — "After leaving the tutorial area, re-enter and return to the platform you first spawn
  on before the rain starts, on any cycle."** The user identified the spawn room (`SU_C04`) and
  asked for more on it — decompiling turned up the whole picture in one pass: `SU_C04` is vanilla's
  own canonical story-mode starting den for every character except Hunter (`LF_H01`) and MSC's
  Sofanthiel (`SH_E01`) (`SaveState.GetStoryDenPosition`'s own fallback case), and it hosts a
  dedicated startup script (`RoomSpecificScript.SU_C04StartUp`, tracking `pushRight`/`stoodUp`) that
  IS the literal "out of control, moving out of the top-left corner" intro sequence the user
  described. Even better, vanilla defines the WHOLE tutorial area's own room list directly —
  `OverseerAbstractAI.DefineTutorialRooms`'s vanilla (non-MMF) branch: `SU_C04, SU_A41, SU_A42,
  SU_A43, SU_A44, SU_A22` (the MMF-only 2 extra gate rooms are skipped, since MSC/MMF must be
  disabled for every one of these challenges per the CSV's own header note) — exactly the "left the
  tutorial area" boundary this challenge needed, found by chasing the user's one room ID rather than
  needing a second question. "Left the tutorial area" = entered any room outside that 6-room list
  (Outskirts as a whole is much bigger than just these 6, so this is a real boundary, not "left SU
  entirely"). This closes most of Vegan's own remaining gap too (same tutorial-area geometry) — see
  Outstanding items below for what Vegan still needs on top of it (the 7 fruit positions + gap
  room).
- **Fishtank — "Bring 7 Hazers from unique spawn locations to the same body of water, alive."** The
  user resolved the original blocker (a Hazer den map) with a simplification instead: distinct
  Hazer INSTANCES stand in for "unique spawn locations," explicitly including grabbing 2 Hazers from
  the SAME room and bringing both — that still counts as 2 of the 7, no per-origin-room dedup at
  all. Checked as a plain snapshot (the same "N creatures alive together in one room" shape
  Safari/Elder already established) on every room arrival: does the player's current room have
  water (`Room.water`, a plain public field) and contain 7+ live Hazers
  (`CreatureTemplate.Type.Hazer`)? Doesn't require one single trip — Hazers accumulating in one
  water room across several separate carries until 7 are alive there together satisfies this exactly
  as well as one trip with all 7 in hand.

**Takeaway reinforcing the user's own new standing instruction from the Heist correction earlier:**
all three of these went from "blocked, needs a map/room ID no source-search could ever find" to
"fully implemented" the moment the exact room ID was asked for directly — confirming again that
these room-ID gaps are a one-question fix, not a research problem. **47 of 50 challenges now have a
tracker registered.** Remaining 3: Vegan (needs the 7 fruit positions + the charge-hop gap room,
now that the tutorial geometry itself is known), Broadcast (still zero technical foothold at all,
independent of room IDs), Saint (still genuinely underspecified by its own rules). None of today's
7 new trackers (Elder/Heist/Triplopia/Outlaw/Journey/Visitor/Fishtank) have been built or tested
in-game — no `.NET` toolchain in this container this session either.

## 2026-09-17 (later still): Vegan and Saint implemented, Broadcast's real mechanism found — 49 of 50

- **Vegan — implemented.** The user supplied both remaining facts: blue fruit (`DangleFruit` —
  vanilla has no separate "blue" variant, it's just that object's own default color) spawns in
  `SU_A44`/`SU_C04`, the charge-hop gap is `SU_A43`, and per the user's own clarification, fruit can
  only be GRABBED in those rooms but EATEN anywhere. `VeganTracker` doesn't gate eating by room at
  all — it just runs a count, then checks that count against 7 at arrival in `SU_A22` (the room
  right after the gap jump, already known from `VisitorTracker`'s tutorial-room list above), per the
  user's own suggested checkpoint. Reaching `SU_A22` with fewer than 7 permanently disqualifies that
  attempt (matching "before crossing... for the first time" — a later re-crossing can't retroactively
  count).
- **Saint — implemented, two paths sharing one restriction/completion core.** The user clarified
  this challenge's own real design: it's built around **Hunter**, whose 3 objectives (visit Five
  Pebbles and sleep, deliver the starting green neuron to the Looks-to-the-Moon iterator, ascend)
  already have vanilla's own save-slot-tracked state behind them — so Hunter's own path reads that
  directly rather than re-detecting it live: `SaveState.miscWorldSaveData.pebblesSeenGreenNeuron`
  (confirmed via decompiling `OracleBehavior.cs` — set specifically when Hunter, carrying the green
  neuron, reaches the Five Pebbles oracle) and `.moonRevived` (confirmed via
  `SLOracleWakeUpProcedure.cs` — set when a neuron delivery wakes Moon), both read at every
  hibernation. Every other character (Survivor included) has no such built-in bookkeeping for these
  specific objectives, so the user asked for manual live tracking instead: "visited Pebbles and
  slept" is a hibernation in an `SS_`-prefixed shelter; "delivered a neuron to Moon" needed a brand
  new shared mechanism, **`SurvivorMoonDelivery`** (new file) — a Postfix on
  `SLOracleBehavior.ConvertingSSSwarmer()` (the same method `EnlightenmentTracker`'s own doc comment
  already traced as "Moon accepting a neuron," but that tracker reads the resulting counter
  passively — Saint needed the LIVE moment instead, since the counter is a lifetime save-file total
  that could already be primed by deliveries from long before this specific attempt began). Both
  paths share the "without killing anything, or eating live prey" restriction, permanently
  disqualifying on either: a new `PrimalKillTracker.OnKillCredited` event (any credited kill, fired
  right alongside that class's existing per-type/per-instance recording) and
  `SurvivorEvents.OnObjectEaten` filtered to `edible is Creature && !creature.dead` — confirmed via
  decompiling `Hazer.BitByPlayer`/`SmallNeedleWorm.BitByPlayer`/`Centipede.BitByPlayer` that vanilla
  itself draws exactly this line: creatures eaten via a `Die()` call before `ObjectEaten` (Centipede,
  SmallNeedleWorm) are "killed then eaten," while ones with no `Die()` in that path (Hazer) are
  eaten genuinely alive — `!creature.dead` at the `ObjectEaten` moment is a real, not approximated,
  signal for this distinction. **Left unimplemented per the user's own explicit instruction:** "no
  karma caching," "no echo-negated kills," and "no Guardian Skip" are all technique-legality clauses
  with no corresponding observable state.
- **Broadcast — the real mechanism found, tracker itself still blocked on 2 room IDs.** The user
  described exactly how a Vulture Grub's signal works (yellow laser + timer, green + vulture if open
  sky is found, red otherwise), and decompiling `VultureGrub.cs` against that description confirmed
  it precisely: `AttemptCallVulture()` sets `vultureCalled = true` only on the branch that actually
  finds a creature and a valid sky path (the "green" case); the "red" failure case is set inline in
  `Act()`'s per-tick update (`callingMode = -1` once the countdown expires with no vulture ever
  called), not its own method. New shared mechanism built: **`SurvivorVultureSignal`** (new file) —
  `OnSignalSucceeded` (a Postfix on `AttemptCallVulture` checking `vultureCalled`) and
  `OnSignalFailed` (a Postfix on `Update(bool)`, guarded by a per-instance `ConditionalWeakTable` so
  the continuously-true `callingMode == -1` state only fires once per attempt, cleared by a third
  Postfix on `InitiateSignal()` — the method that starts a fresh attempt). This mechanism is generic
  (any grub, anywhere), matching this mod's usual "build the shared primitive broad, let each
  tracker narrow it" pattern. **`BroadcastTracker` itself is not written yet** — still needs: the
  exact room ID of "the southmost Sky Islands shelter" (the starting point) and the exact room ID of
  the top of Communications Array (where the death-to-Vulture must happen). Once supplied, wiring
  the tracker itself should be small: arm on hibernation at the start shelter (same shape as
  `JourneyTracker`), disqualify on `SurvivorVultureSignal.OnSignalFailed`, and complete on
  `SurvivorEvents.OnPlayerDeath` in the target room with a Vulture/KingVulture/MirosVulture as the
  killer, while `OnSignalSucceeded` has fired and not since failed.

**49 of 50 challenges now have a tracker registered** (only Broadcast remains, pending the 2 room
IDs above). None of today's 9 new/changed trackers (Journey/Visitor/Fishtank from the previous
section, plus Vegan/Saint here, plus the `PrimalKillTracker`/`SurvivorMoonDelivery`/
`SurvivorVultureSignal` shared-mechanism additions) have been built or tested in-game — no `.NET`
toolchain in this container this session either. `SaintTracker` in particular is the most
structurally complex tracker in the whole file (two independent detection paths plus a shared
restriction plus a shared completion gate) and touches the newest shared mechanism
(`SurvivorMoonDelivery`), so it deserves the most scrutiny of this batch once a real build is
possible.

## 2026-09-17 (later still): Broadcast implemented — all 50 of 50 community challenges now tracked

The user supplied the last 2 room IDs (southmost Sky Islands shelter `SI_S03`, top of Communications
Array `SI_A07`) and answered two design questions that materially changed the tracker's shape from
what was proposed:

1. **The signal does NOT need to succeed** — only an actual failure (red) is disqualifying. Whether
   any grub ever successfully summons a vulture is irrelevant to completion.
2. **The grub must be thrown while still inside the starting shelter (SI_S03).** Leaving that room
   without ever having thrown one there **cancels the attempt outright** (removed from tracking, not
   just left permanently unable to complete) — the user's own words, "the attempt is cancelled."

This needed one more piece of new plumbing on top of the `SurvivorVultureSignal` mechanism built
last round: **`SurvivorVultureSignal.OnGrubThrown`**, a Prefix on `Player.ThrowObject(int grasp,
bool eu)` (confirmed via decompiling `Player.cs` to be the exact, only call site of
`VultureGrub.InitiateSignalCountDown()` — the method that starts the yellow-laser countdown — so
this is the real "thrown" moment, not an approximation off release/drop in general). A Prefix, not a
Postfix, since by the time `ThrowObject` returns the object has already left the grasp being read.

`BroadcastTracker` itself: arms the same way `JourneyTracker` does (hibernating in `SI_S03` arms the
next cycle), then tracks exactly ONE specific `VultureGrub` instance — whichever one
`OnGrubThrown` reports thrown while the player is still physically in `SI_S03` (the most recent one,
if more than one is thrown before leaving). Only THAT grub's own `OnSignalFailed` matters, checked
by reference equality against the tracked instance — an unrelated grub failing somewhere else in the
world has nothing to do with this specific attempt and must not disqualify it. Leaving `SI_S03`
without a tracked grub yet cancels the attempt immediately (removes it from the dictionary entirely,
per the user's own "cancelled" framing). Completes on `Player` death in `SI_A07` to a Vulture or
KingVulture (MirosVulture, a DLC-exclusive variant, is skipped outright — every one of these
challenges requires MSC/DLC content disabled per the CSV's own header note, so it can never actually
appear in a qualifying run), with the tracked grub's signal never having failed.

No race condition between this tracker's own `OnPlayerDeath` handler (which reports completion AND
removes the attempt) and the separate `OnCycleFailed` cleanup handler (which would otherwise also
try to remove it): confirmed via decompiling `RainWorldGame.cs` that `OnPlayerDeath` is raised from
a Postfix on `Player.Die()`, while `OnCycleFailed` is raised from a Postfix on the LATER
`RainWorldGame.GameOver()` — `Player.Die()` always runs first, so completion is always checked
before the cycle-failure cleanup could remove the attempt out from under it.

**All 50 of 50 community challenges now have a tracker registered.** None of this session's 10 new
trackers (Elder, Heist, Triplopia, Outlaw, Journey, Visitor, Fishtank, Vegan, Saint, Broadcast) or
the 4 new shared mechanisms (`SurvivorTollState.OnOutpostTick`, `PrimalKillTracker.OnKillCredited`,
`SurvivorMoonDelivery`, `SurvivorVultureSignal`) have been built or tested in-game — no `.NET`
toolchain in this container this whole session. **The very next thing to do with this branch is get
it through a real `dotnet build`** (on the user's own Windows machine, per the "sessions now run
directly on the user's own machine" section near the top of this doc) before trusting any of it
further — this is 10 brand-new trackers plus 4 new shared mechanisms, none compiled even once, and
several (`OutlawTracker`'s `Menu.StoryGameStatisticsScreen` family, `SaintTracker`'s two-path
design, `BroadcastTracker`'s per-instance grub tracking) are genuinely the most structurally complex
code in this whole community-challenges effort. Treat "should work per source-level review" as
exactly that, not as "confirmed" — this doc's own Gotcha #5 is a standing reminder of how far a
plausible-looking, never-built change can drift from actually working.

1. **First design (discarded):** 9 separate SlugBase-defined "alt" characters (clones of the
   vanilla roster), toggled via a Tab key at runtime, with session-identity forced to match the
   real character so saves/cutscenes would behave. This produced multiple freeze bugs and felt
   fragile.
2. **Second design (discarded):** switch to the reserved save slot (666666) via a 4th Options-menu
   button (this part survived), but still using the 9 SlugBase alt characters — now roster-filtered
   so only alt characters showed on the reserved slot, with session-identity-forcing generalized to
   all 9. User's own words pivoting away from this: *"if we are switching to a new save slot as an
   idea, then we dont need to alt char switcher - on the new save slot, you should only see the alt
   characters... this way, outside the save slot, characters are unaffected."* This was implemented,
   then immediately pivoted again:
3. **Current design:** user's own words: *"can we make alt char save slot use actual vanilla
   characters, thereby not making new characters at all? this way we would be sure that all
   character properties and world states and their interactions are completely in tact - then, if
   we could give a check to whether we are in the 666666 save slot, we could apply the gimmicks
   onto the normal characters. No need to create new characters via slugbase. This seems like the
   most foolproof solution if possible."* All 9 SlugBase characters, the roster-filtering system,
   and the session-identity-forcing code were removed. Gimmicks were rewired to check
   `PrimalSlot.IsActive(rainWorld)` + the player's real vanilla `playerState.slugcatCharacter` identity
   directly, as described above. **This is the architecture to keep building on.**

**Takeaway on user preference:** they've consistently pushed back on speculative/over-engineered
solutions in favor of simpler ones that lean on vanilla's own already-tested machinery instead of
reimplementing it (see also: the Options-menu button reusing vanilla's actual slot-switch code
rather than a custom state machine, after an earlier custom-state-machine attempt corrupted a save
file). When in doubt, prefer the smaller patch that rides on an existing vanilla system over a
larger one that reimplements it.

## 2026-09-17 (later still): review pass -- 5 more mechanics pulled up into shared code

Requested directly: "look back at all of the shared mechanics and trackers and see what can be
abstracted out," citing the Kill List -> PrimalKillTracker extraction as the model. Read all 50
`ChallengeTracker` implementations plus every `Survivor*.cs` shared-mechanism file in full to find
duplication that had crept in across ~15 individual challenge-implementation passes (most of it
added independently, one challenge at a time, with no cross-checking against sibling trackers).
Found and pulled up five real, verbatim-or-near-verbatim duplicates -- deliberately did NOT force
an abstraction onto things that only LOOK similar (e.g. the various "armed at shelter X, disarmed
by Y" trackers -- Journey, Criminal, Broadcast, Devotee -- all differ enough in their actual arm/
disarm conditions that a shared abstraction would hurt more than it helps; left alone, matching
this project's own standing "don't force premature generalization" discipline):

1. **`PrimalKillTracker.ResolveKiller(Creature)`** -- the "check Creature.killTag first, fall back
   to TryGetRecentHandler" resolution was independently duplicated verbatim in three separate
   `Creature.Die` Postfixes: `PrimalKillDieHook` (this class's own original),
   `SurvivorTollRosterDeathHook` (Manslaughter's "no kill credit" check), and
   `TriplopiaPoleMimicDeathHook` (Triplopia's same-den kill tracking, added just this session).
   Three independent copies of the same 8-line block was the exact "second instance is a
   coincidence, third instance is a pattern" trigger -- pulled up, all three call sites now read
   `PrimalKillTracker.ResolveKiller(creature)`.
2. **`SurvivorCreatureTracking.FindCarried(Player, Func<Creature, bool>)`** -- a grasp-array scan
   for a Creature matching some predicate, independently reimplemented in SIX places:
   `PilgrimageTracker.FindCarriedLizardOfType`, `ChefTracker.FindCarriedAlbinoJetfish`,
   `OfferingTracker.FindCarriedTargetMouse`, `SymbioteTracker.FindCarriedSquidcada`,
   `ChargedTracker.FindCarriedBabyCentipede`, and half of `GuardianTracker.TryFindBothCarried`.
   Each converted call preserves its own exact original match condition (including
   `GuardianTracker`'s own `!creature.dead` check, which several of the others never had --
   deliberately NOT added to the others, since that would silently change their behavior, not just
   refactor it).
3. **`SurvivorCreatureTracking.FirstTracked(string)`** -- "the first (only, in 4 of 5 cases)
   creature tracked under this key" was redeclared as an identical private static helper in FIVE
   trackers (Pilgrimage, Chef, Offering, Symbiote, Charged).
4. **`SurvivorCreatureTracking.LiveCreaturesInRoom(AbstractRoom, CreatureTemplate.Type)`** -- "every
   live creature of type X in this room," independently reimplemented in
   `ChieftainTracker.FindNearbyLiveScavengers` and inline in `FishtankTracker`/`SafariTracker`
   (Safari's own single-pass 3-flag scan became three calls to the shared helper -- three room
   scans instead of one, an intentionally-accepted trade for clarity given how small a room's own
   entity list actually is).
5. **`SurvivorEvents.ResetOnCycleBoundary(Action<string>)`** -- the exact pair "subscribe to
   OnSuccessfulHibernation AND OnCycleFailed, each just discarding this character's own per-cycle
   state" was independently repeated in Visions, Mycologist, Nomad, Ornaments (a raw dictionary
   entry each time) and Symbiote (a `SurvivorCreatureTracking.ClearTracker` call each time).
   Composes the two real events so a caller states its own reset action once. **Not** applied to
   trackers where the hibernation side does more than reset (Usurper, Safari, Charged all do real
   completion-checking in their own hibernation handler, not a pure discard) -- those were
   correctly left alone.

**A sixth cleanup, `RegionCodes`** (a new small static class of named region-acronym constants,
`RegionCodes.FivePebbles = "SS"` etc.) replaced ~13 independently-declared private `const string`
region codes across 9 trackers -- Five Pebbles ("SS") alone had been separately typed out as
"FivePebbles" four times, Subterranean ("SB") three times, with no shared source of truth to catch
one of them silently drifting from the others (a typo in just one copy would have been a real,
easy-to-miss bug). Each tracker keeps its own local `private const string FivePebbles =
RegionCodes.FivePebbles;`-style alias rather than being rewired to reference `RegionCodes` at every
use site -- a `const` initialized from another `const` is still a real compile-time constant, so
this cost one line changed per tracker, not a rename throughout each tracker's whole body.

**Net effect:** `ChallengeTrackers.cs` shrank from 3593 to 3345 lines despite the new `RegionCodes`
class and expanded doc comments -- the diff itself is -359/+218 for that file alone. Zero intended
behavior change anywhere in this pass: every extraction preserves each original call site's exact
condition, and this was checked by re-reading every changed call site against what it replaced, not
assumed from the extraction being "obviously equivalent." Still, **this whole pass is itself
unbuilt and untested**, the same standing caveat as everything else in this doc -- a refactor across
15+ files with no compiler available is exactly the kind of change most likely to have a small typo
slip through unnoticed, so give this one particular scrutiny on the next real build, not just the
newest trackers.

## 2026-09-17 (later still): the Requirements architecture -- ChallengeTracker generalized, gated by a performance check, and split into its own Remix tab

Follow-on to the review pass immediately above, this time an architectural discussion rather than
a mechanical dedup. The user asked directly for an opinion on making the tracking hooks global
with gimmicks enabling only what they need (for performance, and because future gimmicks --
MasterSpear needing story% completed, "every shelter visited," "run has to not have had a single
death" -- will need their own success-state conditions that have nothing to do with Community
Challenges). Talking it through surfaced the real distinction: it's not the *tracking mechanisms*
that need per-gimmick gating (SurvivorEvents etc. are already global, cheap, always-on), it's
*what counts as a satisfied condition for a character* that had been wrongly hardwired to one
specific gimmick (Community Challenges) instead of being its own concept. Confirmed with two
worked examples before implementing anything (Visitor: tracker is global, "the requirement is a
separate piece of code... UI wise, you enable this setup by adding a requirement"; Kill List:
tracker is global, gameplay is the Kill List button gimmick, "Chains Kill List" would be a
separate requirement layered on top) -- both matched the design implemented below.

**The perf gate, added first, standalone:** `GimmickAssignments` (previously just a
`(characterID, gimmickID) -> Configurable<bool>` store) gained a live, incrementally-maintained
`Dictionary<string, int> _enabledCount` and `IsEnabledForAnyCharacter(gimmickID)` -- an O(1) read
of "does at least one character have this ID enabled at all," kept in sync by `SetDefault`/
`Register` adjusting it on write and every live `Configurable.OnChange` toggle adjusting it again,
rather than ever being recomputed by scanning. `ChallengeTracker.TryGetEnabledCharacter` (now
`Requirement.TryGetEnabledCharacter`, see below) checks this FIRST -- one dictionary lookup shared
by all 50 (now, any number of) trackers, instead of 50 independent `GimmickGate.IsActiveFor`
resolutions running on every relevant vanilla event on a save where nobody uses any of them.

**The architecture itself: `ChallengeTracker`/`ChallengeTrackerRegistry`/`ChallengeCompletion`
generalized into `Requirement`/`RequirementRegistry`/`RequirementCompletion` (Requirement.cs,
new), with `ChallengeTrackers.cs` renamed to `Requirements.cs` and every one of its 50 classes
mechanically renamed (`XTracker : ChallengeTracker` -> `XRequirement : Requirement`, its
`ChallengeEntry`/`Challenge` property pair collapsed into one `ID` property).** The key change
isn't the rename, it's what a Requirement is gated by: previously a challenge needed BOTH the
CommunityChallenges gimmick assigned to a character AND that specific challenge checked in that
gimmick's own 50-checkbox settings sub-view -- two nested layers for one conceptual choice, that
existed purely because Requirements were invented while building Community Challenges specifically.
Now a Requirement is directly, individually enabled per character through the exact same
`GimmickAssignments` (characterID, ID) -> Configurable<bool> store gimmick assignment already
uses -- confirmed to be genuinely generic under the hood despite its name, and `GimmickGate.
IsActiveFor`/`IsEnabledForCharacter` are equally generic over any ID string, so neither needed any
new plumbing. `TriplopiaRequirement`/`OutlawRequirement` (the two trackers whose own detection
methods are `static`, with no `this` to read an instance `ID` property from) each keep a
`internal const string RequirementID = "..."` alongside the `ID` property, used directly inside
their static methods. `RequirementCompletion` renamed its persistence key from
`SurvivorChallengeCompleted` to `PrimalRequirementCompleted` -- a clean break, not a compat shim,
since nothing has shipped and no real save data exists anywhere with the old key. `OnSatisfied` is
a plain multicast event explicitly designed for multiple independent subscribers -- Community
Challenges' own popup is one; a future MasterSpear gate reacting to its own prerequisite becoming
true would be a second, entirely unrelated one, for a different ID.

**Community Challenges is now a pure CONSUMER, not the owner (CommunityChallengesGimmick.cs,
rewritten).** `ChallengeEntry`/`ChallengeCompletion`/`ChallengeNotification`/`ChallengeTracker`/
`ChallengeTrackerRegistry` are gone from this file entirely (superseded by Requirement.cs). What's
left: `CommunityChallengesRoster` (just the ordered 50-ID list, matching the community's own
ranking, display names sourced from each `Requirement.DisplayName` rather than duplicated),
`CommunityChallengesNotifications` (a thin `RequirementCompletion.OnSatisfied` subscriber, gated
`isLive && CommunityChallengesRoster.Contains(requirementID)`, replacing the old inline
`ChallengeCompletion.ReportCompletion` popup call), and the button/dialog pair, unchanged in
spirit but repointed onto `RequirementCompletion`/`RequirementRegistry`/`GimmickAssignments`
directly. **There is no "Community Challenges" gimmick to assign anymore** -- it's gone from
`GimmickRegistry.All` entirely. The cycle-end CHALLENGES button's own gate changed from "is the
CommunityChallenges gimmick assigned to this character" to `CommunityChallengesRoster.
IsAnyEnabledFor(characterID)` -- does this character have at least one of the 50 curated
Requirement IDs directly enabled. `CommunityChallengesNotifications.Init()` is wired into
`RainWorldHooks.Postfix` alongside `RequirementRegistry.RegisterAll()`, the same explicit
one-shot-call-site pattern this file's siblings already use for plain event subscriptions that
aren't Harmony patches.

**PrimalOptions.cs gained a second Remix tab, "Requirements," fully independent of the existing
"Gimmicks" tab** -- matching the UX the user asked for directly: "open it up, set up the gameplay
altering mechanics that they want, then... set up the requirements needed to complete the run...
everything else should be performantly and neatly automatically handled for them." Structurally a
second, parallel copy of the Gimmicks tab's own permanent-grid-plus-detail-area shape (own `OpTab`,
own `_reqDetailItems`/`_reqDetailView`/`_reqCharID` state, deliberately NOT folded into the
Gimmicks tab's own `DetailView` enum -- a requirement isn't a kind of gimmick), but with a single
detail sub-view (a 5-column/10-row checkbox grid, one row per registered Requirement, wrapped in
the same `OpScrollBox` pattern `RenderGimmickSettings` already uses) rather than three -- there's
no per-requirement settings drill-in needed yet, though the structure leaves room for one later.
`BuildCharacterGrid` was generalized to take a click callback and description prefix so both tabs
share the exact same character-grid-building code without duplicating it. `EnsureConfigBound()`
grew a second loop (per character, per `RequirementRegistry.All` entry) binding a
`Configurable<bool>` and calling `GimmickAssignments.Register` for the pair -- unlike gimmicks,
every requirement defaults to OFF for every character (nothing calls `GimmickAssignments.
SetDefault` for a requirement ID), since these are meant to be explicitly opted into per run, not
pre-enabled.

**Not yet built or tested** -- no .NET toolchain in this container, same standing caveat as
everything else in this doc. This is a genuinely large mechanical + structural change (a 3300+
line file's worth of renames, a new ~320-line file, a full rewrite of the old gimmick file, and a
~150-line addition to the options screen) with no compiler available to catch a typo -- give the
whole Requirements/Requirement.cs/CommunityChallengesGimmick.cs/PrimalOptions.cs set particular
scrutiny on the next real build.

## 2026-09-17 (later still): Remix menu redesign -- character combo box, split gimmicks/requirements view, in-tab "modals"

Requested directly: replace the character-button grid with a select control to save space, split
the screen vertically (gimmicks left, requirements right), and turn the "add" flow into a
window/modal holding a 4-column scrollable picker. Confirmed three design points before
implementing: (1) the split collapses the Gimmicks/Requirements tabs into one tab -- the user
believed only the Gimmicks tab currently existed anyway; (2) clicking an assigned gimmick's name
now opens its settings in its own modal, rather than taking over half the split view; (3) clicking
a picker entry assigns it and closes the modal immediately (add one at a time).

**Character grid -> OpComboBox.** The old 5x2 button grid (`BuildCharacterGrid`, ~95px tall) is
gone, replaced by one `Menu.Remix.MixedUI.OpComboBox` bound to a new mod-wide
`Configurable<string>` ("SelectedCharacter") -- which also means the screen now remembers which
character was last selected across restarts, for free, through the same Remix-config mechanism
every other setting on this screen already uses. `OpComboBox`'s 4-arg constructor
(`Configurable<string>, Vector2, float, List<ListItem>`) was used specifically because it doesn't
require the Configurable to carry a `ConfigAcceptableList<string>` (that's only needed by the other
constructor overload that builds its own `List<ListItem>` from `config.info.acceptable`) --
`ListItem(id, displayName, index)` lets the combo box show "Gourmand" while storing
`MoreSlugcatsEnums.SlugcatStatsName.Gourmand.value` and preserves the roster's own declared order
(each entry's `value` field is set to its index, so `ListItem.Comparer`'s stable sort reproduces
that order instead of falling back to alphabetical). One subtlety caught before it shipped:
`Configurable<string>.OnChange` can fire before `Initialize()` has ever run (Remix's own automatic
`config.Reload()` pass, same one `EnsureConfigBound`'s own doc comment already traces, can apply a
persisted "last selected character" value at `RainWorld.PostModsInit` time, long before the player
opens this mod's Remix page) -- `OnCharacterSelectionChanged` updates `_charID` unconditionally but
guards the actual `RenderDetail()` call behind `_tab != null`, so it's a no-op until the tab
actually exists; `Initialize()` reads `_selectedCharacterConfig.Value` directly afterward regardless.

**Two tabs -> one tab, vertically split.** `_tab`/`_reqTab` and their fully-duplicated
detail-item/detail-view state collapsed into a single `_tab`/`_detailItems`/`DetailView`. The
`Split` view (the normal, no-modal state) renders both columns from the SAME selected character in
one pass: a `GIMMICKS` header + scrollable assigned-list + "+ Add Gimmick" button on the left
(`LeftColumnX`), the same shape for `REQUIREMENTS` on the right (`RightColumnX`) -- each column is
`(ContentWidth - ColumnGap) / 2` wide. A gimmick's row is a clickable name button (opens its
settings) plus an "x" remove button, matching the pre-redesign table exactly; a requirement's row
is a plain, non-clickable label plus the same "x" -- there's no requirement-settings screen to open
yet (the user confirmed no per-requirement settings are needed right now), and a button that opens
nothing would just be a dead click target. Building two OpScrollBoxes in one pass repeats the same
two-step commit order every scroll box on this screen has needed since 2026-09-15 (attach the
still-empty scroll box to the tab via ONE `ShowDetail` call first, THEN call each scroll box's own
`AddItems` and mirror those rows into `_detailItems` -- `OpScrollBox.AddItems` forwards straight to
`_tab.AddItems`, not to the scroll box's own children, so skipping the mirroring step leaks
permanently-attached, still-clickable rows into the next view, the exact bug already fixed once for
the old single-scroll-box Settings/AddGimmick sub-views).

**"Modal" windows, and what that actually means in Remix's own UI toolkit.** The three non-Split
detail views (`GimmickSettings`, `AddGimmick`, `AddRequirement`) render as full-width overlays with
a bordered backdrop (`AddModalBackdrop`, a semi-transparent `OpRect` sized to the whole detail
area) and a "< Back" button, replacing the split content entirely rather than sitting alongside it
-- structurally this is still the exact same `ShowDetail` swap the old Settings/AddGimmick
sub-views already did, just visually dressed up to read as a distinct window. **A genuinely
separate window (a real `Menu.Dialog` side-process, the mechanism
CommunityChallengesDialog/KillListDialog already use elsewhere in this mod) was deliberately ruled
out**: every Remix modder-facing control this screen and every existing gimmick's own settings UI
is built from (OpCheckBox, OpScrollBox, OpComboBox, ChainsSettings' banlist grid, ...) only exists
inside an OpTab/ConfigContainer -- none of it can be hosted inside a `Menu.Dialog`, which only
understands plain `Menu.MenuObject`-derived controls (`Menu.SimpleButton`, `Menu.MenuLabel`, the
vocabulary CommunityChallengesDialog is written in). Rebuilding every existing gimmick's settings
screen in that vocabulary just to get a true floating window would have been a far larger rewrite
than this pass asked for, so the in-tab-overlay approach was chosen instead and is called out
explicitly here and in PrimalOptions.cs's own header comment so a future pass doesn't mistake this
for an oversight.

**The add-picker modal** (`RenderAddGimmickModal`/`RenderAddRequirementModal`, structurally
identical to each other -- not shared through a generic helper, since this codebase already
avoids `ValueTuple`-shaped generic plumbing on principle, see GimmickAssignments.cs's own header
comment on that) lists every NOT-currently-assigned/enabled entry in a 4-column, scrollable,
row-major grid (`PickerColumns = 4`); clicking one assigns it and calls `CloseModal()`-equivalent
logic (`AssignGimmick`/`AssignRequirement`) that also switches back to Split immediately, matching
the "assign and close" behavior confirmed before implementing. Gimmick buttons keep their existing
home-character border coloring (`GetHomeCharacterColor`); requirements have no such concept, so
their picker buttons are plain.

**Not yet built or tested** -- no .NET toolchain in this container. This is a full rewrite of
PrimalOptions.cs's layout math (every constant from the old grid/tab/settings-viewport era was
replaced), so give it particular scrutiny on the next real build -- especially OpComboBox's own
runtime behavior (this is the first time this mod has used that specific Remix control) and
whether four-column picker buttons fit the longest gimmick/requirement display names
("Threat Slowdown", "Enlightenment") without clipping.

## 2026-09-17 (later still): a mode-selection gate -- Primal (read-only) vs. Custom (editable)

Requested directly: gate the whole Remix screen behind a first-run choice between "Primal mode"
(view-only) and "Custom mode" (exactly the existing editable screen), shown as a title/credits/
description blurb followed by two rows (a button on the left, explanatory text to the right), with
the choice persisted so it's skipped on every later visit, and a "Change Mode" button in the
normal view's own top-right corner to revisit the choice. Confirmed before implementing: only two
modes exist (the "3 choices" in the request was a typo), and Primal mode simply freezes whatever
is currently configured against further edits rather than snapping to some separate curated preset
-- there's no second config to design or maintain.

**New top-level screen state, above (not instead of) everything the previous UI-redesign pass
built.** `ScreenState` (`ModeSelect` / `Normal`) now sits above `DetailView` (`Split`/
`GimmickSettings`/`AddGimmick`/`AddRequirement`, all unchanged) -- ModeSelect is a whole different
screen (title/credits/description/two choice rows), Normal is everything that already existed
(the permanent title/character-select row plus the Split view and its modals). Three separate
tracked item lists now exist on the one `_tab` -- `_modeSelectItems`, `_permanentItems` (renamed
from the anonymous local `permanentItems` Initialize() used to build inline), and the existing
`_detailItems` -- because switching between ModeSelect and Normal has to tear down and rebuild the
permanent row too, not just the detail area the way a plain Split<->modal switch within Normal
already did. `ClearAllScreens()` removes whichever of the three is currently populated;
`ShowModeSelect()`/`ShowNormal()` call it before rebuilding their own content.

**Persistence, and the one real subtlety in wiring it up.** A new `Configurable<string>
ScreenMode` ("" / "Primal" / "Custom") is bound in EnsureConfigBound exactly like every other
setting on this screen. Initialize() reads it directly (not via an OnChange handler, unlike the
character-select Configurable) since it only needs a one-time decision at startup: empty means
show ModeSelect, anything else jumps straight to Normal. Selecting a mode calls the same
"immediately, unconditionally call config.Save() via reflection" pattern SetEnabledFor's own long
doc comment already traces in full (SetEnabledFor's own tail was pulled out into a new shared
`SaveConfigNow(whatChanged)` helper rather than duplicating that AccessTools.Method Invoke/try-catch
a second time). "Change Mode" deliberately does NOT touch the persisted value -- it just calls
`ShowModeSelect()` to let the player look at the chooser again; only an actual click on one of the
two mode buttons writes to `ScreenMode`. That's what makes "skip the mode question next time" mean
what it should: reopening Remix later re-reads whatever was last actually chosen, regardless of
whether the player visited-but-didn't-repick the chooser mid-session.

**Enforcing read-only, and why it needs two separate mechanisms.** Primal mode isn't just a label
-- every control that would change an assignment has to actually stop responding. Confirmed by
reading OpSimpleButton/OpCheckBox/OpComboBox's own decompiled Update() methods: `greyedOut` isn't
cosmetic dimming, it's Remix's own real interactivity gate (`if (greyedOut) { ...; return; }` skips
the entire click-handling branch, so `OnClick` never fires at all). So: the Split view's own
`+ Add`/`x` buttons get `greyedOut = IsReadOnly` set at construction; a gimmick's own settings
sub-view is a different story, though -- ChainsSettings' banlist checkboxes and every other
gimmick's own settings controls write straight to their own bound Configurable on interaction,
never routing through this screen's own SetEnabledFor at all, so RenderGimmickSettingsModal
post-processes whatever `GimmickDef.BuildSettingsUI` returns and sets `greyedOut = true` on every
element that's a `UIfocusable` (OpLabel isn't one, and needs no such flag -- it was never
interactive to begin with). On top of both of those, `SetEnabledFor` itself now also no-ops while
`IsReadOnly` -- a second, UI-independent guarantee that holds even if some future control forgets
to set `greyedOut`, and `RenderAddGimmickModal`/`RenderAddRequirementModal` bail straight back to
Split if somehow reached while read-only (shouldn't be possible through the UI, since their own
"+ Add" buttons are greyed out, but cheap defense-in-depth matching this file's own established
style). The character-select combo box and "Change Mode" button are deliberately NEVER greyed --
switching characters or navigating back to the mode chooser isn't "changing the configuration."

**Layout.** The title row's top-right corner, previously the slot-switch key, is now the "Change
Mode" button (plus a small "Mode: Primal (read-only)" / "Mode: Custom" indicator) -- the
slot-switch key moved down to its own row (`SlotSwitchY`), and the character-select row shifted
down to match (`CharSelectY`), which in turn shifts `DetailTopY` and everything derived from it
down by the same amount; still comfortably tall enough for the Split view's two scrollable columns.
The ModeSelect screen's own title/author/description/choice-row Y positions
(`ModeAuthorY`/`ModeDescriptionY`/`ModeChoiceHeadingY`/`ModeChoiceRow1Y`/`ModeChoiceRow2Y`) are all
derived from `TitleY` by simple constant arithmetic, verified by hand to leave a comfortable,
non-overlapping gap between every element and stay well clear of both screen edges. The credits/
description text itself is read live from `mod.name`/`mod.version`/`mod.authors`/`mod.description`
(i.e. modinfo.json) via `OptionInterface`'s own inherited `mod` property, rather than a second,
easy-to-drift hand-typed copy of the same text -- an `OpLabelLong` (not the plain single-line
OpLabel used everywhere else on this screen) renders it with real word-wrap.

**Not yet built or tested** -- no .NET toolchain in this container, and this is the first time this
project has used a Configurable-driven boolean gate that greys out OTHER gimmicks' own
independently-written settings UI after the fact, rather than each gimmick controlling its own
interactivity -- worth particular scrutiny (does `UIfocusable.greyedOut` genuinely block every kind
of control this mod's various gimmicks return, not just the button/checkbox cases confirmed by
reading source) on the next real build.

## 2026-09-17 (later still): four character-specific requirements (Artificer, Gourmand, Spearmaster, Inv)

Requested directly, one per character: Artificer kills every Kill List species and ascends;
Gourmand completes the vanilla "food quest" and saves the cycle; Spearmaster gets the story ending
(not the void-sea one) in under an in-game hour; Inv reaches LC_highestpoint while holding a
slugpup, a mask, and a cloak. Added as four new `Requirement` subclasses at the bottom of
Requirements.cs, registered in `RequirementRegistry.All` alongside the 50 curated Community
Challenges -- these were never part of that community list, so they get their own short doc-comment
section explaining why each is gated to one specific character (`characterID == <SlugcatStatsName>
.value` inside the detection code itself, the exact pattern `OutlawRequirement` already established
for Hunter-only completion -- `TryGetEnabledCharacter` only confirms the requirement is enabled and
the player is on a reserved slot, not which real character it resolved to). Nothing stops enabling
one of these for the "wrong" character via the Add Requirement picker; it will simply never
complete, the same pre-existing, accepted OutlawRequirement behavior.

**ArtificerKillAscendRequirement.** "Ascend" is `standardVoidSea == true` on
`SurvivorEvents.OnGameEnding` -- the same flag `AscensionRequirement` already reads, confirmed via
`SlugpupGimmick.cs`'s own long trace of what that flag actually means (the separate void-sea/
ascension ending vs. every "standard" one). "The specified list of creature types found under the
Kill List gimmick" is `KillListRegistry.All`'s own 55-species roster (`CreatureKillListGimmick.cs`)
-- checked via `PrimalKillTracker.IsKilled` for every entry, the same global, permanent, ever-on-
this-save kill record Ranger/Spearfisher already read, deliberately independent of whether the Kill
List GIMMICK itself is assigned (only its roster DATA is reused).

**GourmandFoodQuestRequirement.** "The food quest" turned out to be vanilla's own real,
already-implemented mechanic -- `WinState.GourFeastTracker` (`MoreSlugcatsEnums.EndgameID
.Gourmand`), the checklist behind the "Gourmand Passage": one progress slot per required food/
creature, `GoalFullfilled` true once every slot has been eaten at least once. Read straight off
`DeathPersistentSaveData.winState` via the same `GetStorySession?.saveState?
.deathPersistentSaveData` chain every other save-context reader in this codebase already uses --
nothing to reimplement. Checked on every `OnSuccessfulHibernation` ("and save cycle") rather than
only the exact hibernation where the last food item happens to get eaten, since `GoalFullfilled`
means "ever complete," not "completed this specific cycle."

**SpearmasterFastEndingRequirement.** The mirror image of `AscensionRequirement`: wants
`standardVoidSea == false` (any standard story ending, not the void-sea path) within 1 in-game hour
instead of `== true` within 38 minutes. Confirmed `RainWorldGame.clock` only ever increments (no
reset call anywhere in the decompiled `RainWorldGame.cs`) before trusting it as a whole-session
running total the same way `AscensionRequirement`'s own `TimeLimitTicks` already does -- 1 hour =
`60 * 60 * 40` ticks at the nominal 40 ticks/second.

**InvHighestPointRequirement.** `LC_highestpoint` is confirmed a real, single, dedicated room (LC =
Metropolis, Inv's own home region) via decompiling `GhostWorldPresence.cs`'s per-region ghost/echo
table -- so simply entering it IS reaching that named location, no in-room position math needed,
the same "arrived in room X" shape `VisitorRequirement`/`FishtankRequirement` already use. The
three simultaneous carries are confirmed possible under vanilla's own rules by decompilation before
implementing, not assumed: a slugpup (`CreatureTemplate.Type.SlugNPC`) and a mask (`VultureMask`, a
`PlayerCarryableItem`) each occupy one of the player's two hand grasps, while the "cloak" (the robe
the Moon iterator gives, `MoreSlugcatsEnums.AbstractObjectType.MoonCloak`) is worn via
`SaveState.wearingCloak` -- a plain persistent bool flag set the instant the object is picked up
(`Player.cs`), not a grasp slot at all -- so two hands genuinely suffice for all three items at
once, with no third grasp needed.

**Not yet built or tested** -- no .NET toolchain in this container, same standing caveat as
everything else in this doc. All four rely on real vanilla mechanics (WinState's own passage
tracker, SaveState.wearingCloak, RainWorldGame.clock) confirmed by reading the decompiled source
directly rather than guessed at, but none of the four has been exercised in a live game.

## 2026-09-17 (same day, later): InvHighestPointRequirement corrected -- highest SCREEN, not just the room

User correction: "reaching the highest point in LC_highestpoint room" actually meant the highest
camera SCREEN within that room, not merely entering the room at all. A Rain World room can span
multiple screens (`Room.cameraPositions`, one `Vector2` per screen -- its own bottom-left
world-space corner, each a fixed 1366x768) -- reused `Room.CameraViewingPoint(Vector2)`, vanilla's
own "which screen index contains this point" helper (confirmed via decompilation, already used
internally for exactly this kind of point-to-screen resolution), rather than reimplementing the
box-containment math by hand. "Highest" is whichever screen(s) have the largest
`cameraPositions[i].y` (room-space Y increases upward, confirmed by that same method's own
containment test), compared with `>=` rather than a single tracked index so a top row spanning more
than one screen still counts reaching any of them.

Also switched detection from "check once at room entry" (the original implementation) to "check
every tick while genuinely inside this one room" -- a Postfix on `Player.Update(bool)`, the same
hook TimeLimitGimmick/ChainsGimmick already patch, gated immediately on the room name before any
other work. Reaching the literal top screen is unlikely to be the very first thing that happens on
arrival, so an entry-only check would have missed the realistic "arrive low, climb up" case.
Detection now lives in a static `CheckPlayer(Player)` method with its own `RequirementID` const
(the same static-method-needs-a-const shape `TriplopiaRequirement`/`OutlawRequirement` already
use), called from a new top-level `InvHighestPointUpdateHook` Harmony patch class; `Register()` is
correspondingly a no-op. The item-carrying checks (slugpup + mask in the two hand grasps, cloak via
`SaveState.wearingCloak`) are unchanged from the original pass.

Still unbuilt/untested -- no .NET toolchain in this container.

## 2026-09-17 (same day, later still): character icons added alongside the existing colors, Community Challenges get a white slugpup/batfly tag

Requested directly: gimmicks and requirements should carry both a color AND an icon based on which
character they're designed for, with the 50 curated Community Challenge requirements specifically
getting white + a slugpup icon (batfly as a coded fallback if that one can't be resolved).

**There is no per-playable-character icon SHAPE in vanilla's own kill-icon atlas.** Checked before
assuming otherwise: decompiling `CreatureSymbol.SpriteNameOfCreature` shows `CreatureTemplate.Type
.Slugcat` is the only case that returns the generic `"Kill_Slugcat"` silhouette, and no other
playable-character type (Survivor/Hunter/Gourmand/Artificer/...) has its own distinct icon --
color is the only thing that actually differentiates one character's tag from another's in-game,
which is exactly why the existing gimmick-row treatment was already color-only. So the new icon
system reuses that same single "Kill_Slugcat" shape for every character-tagged row, tinted per
character -- unifying color and icon into one lookup rather than inventing new per-character art.

**`GetHomeCharacterColor` generalized from `GimmickDef` to plain `SlugcatStats.Name`** so
`Requirement.HomeCharacter` (new, mirrors `GimmickDef.HomeCharacter` exactly, `null` by default)
can share the exact same reflection/cache/try-catch instead of a second copy. The four
character-specific requirements (Artificer/Gourmand/Spearmaster/Inv, added earlier this same day)
now each override it to their own character.

**Community Challenges' own white/slugpup(-or-batfly) treatment** lives in
`PrimalOptions.GetRequirementIcon`, checked first via `CommunityChallengesRoster.Contains(id)`
before falling back to `HomeCharacter` -- none of the 50 curated challenges set a `HomeCharacter`
of their own, so without this check they'd simply render with no icon at all. The slugpup icon
itself is a genuine attempt-then-fallback, not an assumption: `MoreSlugcatsEnums
.CreatureTemplateType.SlugNPC` is an MSC-only ExtEnum value (only even attempted while
`ModManager.MSC` is true), and the resolved sprite name is double-checked against
`Futile.atlasManager.DoesContainElementWithName` (the same check `OpImage`'s own constructor
performs) before being trusted -- falling back to `CreatureTemplate.Type.Fly` ("Kill_Bat", vanilla
base-game, always available) otherwise, exactly per the request. In practice `SlugNPC` also
resolves to plain `"Kill_Slugcat"` (the same sharing described above), so the two paths converge on
one shape either way -- white vs. real character colors is what actually reads as the distinction
between "a Community Challenge" and "a character-specific requirement/gimmick," not shape.

**Rendering**: a new `RowIcon` struct (sprite name + color) and `BuildIconElement` (constructs an
`OpImage` from an atlas element name, wrapped exactly the way `OpImage`'s own atlas-string
constructor expects, then normalizes it to a fixed 24px on-screen size regardless of the source
sprite's own native pixel dimensions -- `Kill_Slugcat` and `Kill_Bat` aren't necessarily the same
size) are shared by all three places a gimmick/requirement is ever drawn: the Split view's own
assigned-list rows (icon added to the left, name button/label and the "x" remove button all shifted
right by `RowIconSize + RowIconGap` to make room -- `ColumnNameButtonW` shrunk by the same amount)
and both Add*Modal pickers' 4-column grids (icon at each cell's left edge, its own button narrowed
and shifted right to match). A requirement row's name, previously a plain `OpLabel` with no color at
all, now also picks up the icon's own color when one applies (matching the existing gimmick-row
precedent of tinting both the icon and the name).

**Not yet built or tested** -- no .NET toolchain in this container. Worth particular scrutiny on
the next real build: whether `Kill_Slugcat`/`Kill_Bat` actually look reasonable at the normalized
24px size inside Remix's own coordinate space (never tried before in this codebase -- every prior
icon usage, KillListDialog's own 32px icons, lives inside a `Menu.Dialog`'s native `FSprite`
coordinate space, not an `OpImage` inside an `OpTab`/`OpScrollBox`), and whether the narrowed
4-column picker cells still fit the longest gimmick/requirement display names without clipping now
that they're a little tighter than before.

## 2026-09-17 (same day, later still): Primal-mode character-select portrait override

Requested directly: while on the Primal slot with Remix set to read-only Primal mode, swap a
character's select-screen portrait between mod-supplied "primal-[name]-default" and
"primal-[name]-complete" asset folders -- "complete" once every Requirement currently assigned to
that character has been satisfied, falling back to vanilla's own normal portrait if "default" is
missing, and vanilla's own "story ending" (not the void-sea one) if "complete" is missing. Went
through two rounds of clarification before implementing, both of which changed the design
materially -- see the exchange itself for the full reasoning; summarized here is what actually got
built, in `PrimalPortraitOverride.cs` (new).

**Portraits are multi-layer scenes, not single images, and there are three of them per character,
not two.** Confirmed via decompiling Menu/SlugcatSelectMenu.cs and Menu/MenuScene.cs: each
character has a default scene, a distinct "story ending" scene (`SlugcatPage.AddAltEndingImage()`,
vanilla's own "Slugcat End_B - X"/"AltEnd_X" folders -- exists for Survivor, Monk, Gourmand,
Artificer, Rivulet, Spearmaster), and a separate true void-sea/ascension scene
(`SlugcatPage.AddImage(true)`, "Ghost_X"/"Slugcat End - X"). The user's own "non-void-sea ending"
fallback maps to the STORY ending specifically, not the ascension one -- an important correction
from my own first-pass assumption that vanilla only had two states. Hunter, Saint, Inv, and Watcher
have no distinct story-ending asset, so they fall back to the ascension one instead as the closest
available "finished" portrait, documented as a known simplification.

**"Complete" needed its own new, permanently-persisted marker, not a live recomputation.** The
character-select screen shows every real character at once, but vanilla itself only fully loads
ONE character's own save data into memory at a time (`RainWorld.progression.currentSaveState`);
every other character's fields get read by regex-scraping the raw save-file text in memory
(`SlugcatSelectMenu.SaveGameData.MineForSaveData`'s own slow path, confirmed via decompilation) for
a fixed whitelist of known field names our own Requirement markers aren't on. Per the user's own
suggestion ("whether you met the requirement should be stored as a saved value once you did"),
`RequirementCompletionSummary` (Requirement.cs's `OnSatisfied` event, extended with the
`DeathPersistentSaveData` parameter it already had internally so this consumer doesn't need a live
Player to re-derive it) computes "has this character satisfied every currently-assigned
Requirement" the instant it can, and permanently marks it via `PrimalSaveMarkers` -- reducing the
menu-time question to a plain substring search (`PrimalSaveMarkers.Add` writes markers as literal
strings embedded verbatim in the raw save text, confirmed via decompiling
`DeathPersistentSaveData.ToString()`/`.FromString()`) against `PlayerProgression
.GetProgLinesFromMemory()`'s own per-character raw lines -- which works for every character on the
slot, not just whichever one is "active," exactly what was asked for in the second clarification
round.

**The redirect mechanism itself, and why it patches AssetManager rather than MenuScene's own
per-character scene-building code.** Every layer of a portrait scene is its own PNG, loaded via
`MenuIllustration`/`MenuDepthIllustration`'s shared `LoadFile`, which calls
`AssetManager.ResolveFilePath(string)` synchronously at construction time (confirmed via
decompilation -- not deferred). Patching that one method (Prefix, rewrites the path before the
original runs) lets vanilla's own scene-building code run completely unmodified -- it never needs
to know it's loading from a different folder -- and tolerates the user's own stated intent to ship
a different NUMBER of layer files than vanilla's own scene expects, which reimplementing
`MenuScene`'s hardcoded layer lists would not have. Since that method has zero context of "which
character's portrait is being built," a second pair of patches on `SlugcatPage.AddImage`/
`AddAltEndingImage` brackets an "ambient context" (character, complete-or-not) for exactly the
duration of that one synchronous scene-build call -- safe specifically because the loading is
confirmed synchronous and nested entirely inside it. Those same two patches also FORCE which
vanilla method actually runs and with what `ascended` value, based purely on our own "complete"
decision, deliberately overriding vanilla's own ascended/altEnding dispatch entirely while gated --
per the user's own framing ("we want the framework to implement all character... portraits"), this
is a full override, not a supplement layered on top of vanilla's own three-state system.

**Known simplifications, flagged explicitly in the file's own header comment too:** Hunter's own
"Dead" scene sub-variant isn't distinguished from its normal default; Watcher's four distinct
endings (A/B/C/D) collapse to a single "A" fallback; Inv's scene splits its own layers across TWO
vanilla folders ("Slugcat - White", reused from Survivor, plus a separate "Inv Screen" folder for
just its body sprite) -- both redirect to the same "primal-Inv-*" folder, so one custom folder is
expected to supply every layer Inv's scene needs, not mirror vanilla's own two-folder split.
`slugcatNumber` (genuinely new surface for this codebase) is read via `AccessTools.Field`, not a
direct field access, per this project's own standing Gotcha #1 discipline.

**Not yet built or tested** -- no .NET toolchain in this container. Flagged as the most speculative
patch in this mod so far: `AssetManager.ResolveFilePath` is called constantly throughout the ENTIRE
game, not just this one screen, so its own Prefix's fast-path bailout (ambient context inactive)
deserves particular scrutiny, along with whether the ambient-context bracketing genuinely holds for
every code path that can reach `AddImage`/`AddAltEndingImage` (only `SlugcatPageContinue` and
`SlugcatPageNewGame` were traced; a third caller this pass didn't find could see stale context).

## 2026-09-17 (same day, later still): first real build of the day's whole 15-commit batch found one compile error

Ran `dotnet build` directly in this environment (real toolchain now available, see the 2026-09-15
"sessions now run directly on the user's own Windows machine" section) against everything committed
today from "Elder/Heist/Triplopia/Outlaw trackers" through "Primal-mode character-select portrait
override" -- the whole Requirements architecture, the Remix UI redesign, the mode gate, per-character
icons, and the portrait override, none of which had been built or tested per each section's own
closing caveat. Found exactly one compile error, in `InvHighestPointRequirement.CheckPlayer`
(`Gimmicks/Requirements.cs`): `creature.creatureTemplate` (the predicate runs over a realized
`Creature`, which has no such member -- needs `creature.abstractCreature.creatureTemplate`, the same
pattern every other tracker in this file already uses) and `CreatureTemplate.Type.SlugNPC` (not a
real member -- the correct value is the MSC ExtEnum `MoreSlugcatsEnums.CreatureTemplateType.SlugNPC`,
already used correctly elsewhere in this same file's own header comment's citation and in
`PrimalOptions.CommunityIconSprite()`). Fixed both on one line; also corrected the stale
`CreatureTemplate.Type.SlugNPC` mention in this class's own header doc comment. Rebuilt clean (0
errors, 0 warnings) immediately after.

**This does not mean the rest of today's batch is now verified** -- a clean compile only proves the
other ~14 commits' worth of new code is syntactically and type-correct, not that any of it behaves
correctly in-game (OpComboBox's runtime behavior, the mode-gate's greyedOut enforcement, the icon
sizing, the portrait-override AssetManager patch, and all four new character-specific requirements
are all still exactly as untested as each section above already says). Still needed: an actual
in-game pass, ideally in the order the "Outstanding items" table below already recommends for
large batches -- does it load, does the Remix screen open, then work down from there.

## 2026-09-17 (same day, later still): first in-game launch of the day's batch found two real bugs -- a Harmony load failure and a Gotcha #4 repeat

First actual in-game run of today's whole 15-commit batch (Requirements architecture through the
portrait override). Found two independent bugs, both fixed and redeployed.

**Bug 1 -- Primal failed to load entirely, via a Harmony `AmbiguousMatchException`.**
`BepInEx/LogOutput.log` showed `Primal v0.1.0 FAILED TO LOAD: HarmonyLib.HarmonyException:
Ambiguous match for HarmonyMethod[(class=Spear, methodname=LodgeInCreature, ...)]`, from
`SurvivorPinningHook` (`Gimmicks/SurvivorPinning.cs`, part of the Ornaments requirement's pinning
tracker, added 2026-09-16). `[HarmonyPatch(typeof(Spear), nameof(Spear.LodgeInCreature))]` names the
method with no argument types, but `Spear.LodgeInCreature` is genuinely overloaded (confirmed via
`reference/GameSource/Spear.cs`: a real 3-arg implementation, `(CollisionResult, bool, bool
isJellyFish = false)`, plus a real 2-arg wrapper that just forwards into it) -- Harmony's own
`AccessTools.DeclaredMethod` can't pick one without argument types and throws, which aborts the rest
of that `Harmony.PatchAll()` call inside `Plugin.OnEnable()`'s try/catch. **This is worse than "one
gimmick doesn't work"**: patches for whichever Harmony patch classes the reflection-order enumeration
hadn't reached yet never got applied either that run -- `RainWorldHooks` (which registers the Remix
options screen) happened to run first, which is the only reason the user could still open Remix and
hit Bug 2 below in the same session. Fixed by adding explicit argument types to the `HarmonyPatch`
attribute (`new Type[] { typeof(SharedPhysics.CollisionResult), typeof(bool), typeof(bool) }`),
targeting the 3-arg version specifically since the 2-arg one always delegates into it.

**Bug 2 -- reopening Custom mode a second time crashed the Remix screen with `Menu.Remix.
MultiuseConfigurableException: Configurable SelectedCharacter is bound to multiple UIconfigs`.**
Exact repro: pick Custom mode, click "Change Mode," pick Custom again -> crash, `OptionInterface`
shown replaced with Remix's own red error screen. This is a second, independent instance of
**Gotcha #4** (see that section above) inside the 2026-09-17 Remix-redesign work:
`BuildPermanentItems()` (called fresh every time `ShowNormal()` runs, which includes every
ModeSelect -> Normal transition) built a brand-new `OpComboBox` against `_selectedCharacterConfig`
every single call, never accounting for the fact that `ClearAllScreens()`'s `RemoveItems()` only
detaches the previous OpComboBox from the tab's display list -- it does not unbind it from the
Configurable, so the second `OpComboBox` construction throws the instant it runs, exactly the same
mechanism Gotcha #4 already documents for gimmick settings sub-views. `PrimalSettings
.BuildSlotSwitchBinder`'s `OpKeyBinder` (the screen's other Configurable-backed permanent-row
element) was already correctly cached/reused -- only the newer character-select combo box, added in
the same redesign pass that introduced ModeSelect/ShowNormal churn, had the bug. **Fixed** by adding
a `_characterSelectBox` field and building the `OpComboBox` lazily once (`_characterSelectBox ??
(_characterSelectBox = new OpComboBox(...))`), matching `BuildSlotSwitchBinder`'s own established
pattern exactly.

Both fixes compiled clean and were redeployed to the actual game install
(`...\StreamingAssets\mods\Primal\plugins\Primal.dll`). **Not yet re-tested in-game** -- next check
should be the exact repro sequence that found Bug 2 (Custom -> Change Mode -> Custom, repeatedly),
plus confirming `BepInEx/LogOutput.log` now shows `Primal 0.1.0` loading without the FAILED TO LOAD
line from Bug 1.

## 2026-09-17 (same day, later still): character-select combo box did nothing live -- Remix's own Configurable write-through is deferred, not immediate

User report after the Bug 2 fix above: picking a character in the new combo box visibly changed
nothing, but closing the whole Remix screen and reopening it *did* show that character's own
gimmicks/requirements -- i.e. the value silently WAS taking effect, just not until a full
close/reopen cycle.

**Root cause, confirmed by decompiling the actual Remix pipeline (`Menu.Remix.MixedUI.UIconfig`,
`Configurable<T>`, `OptionInterface`), not guessed:** an `OpComboBox` (or any `UIconfig`) has its
own private UI-side `value` string, separate from the `Configurable<T>.Value` it's bound to.
Clicking an item only ever updates that UI-side `value` -- it does **not** write through to the
bound `Configurable<T>` (and so never fires `Configurable<T>.OnChange`, the event
`OnCharacterSelectionChanged` was listening for) until `OptionInterface._SaveConfigFile()` runs,
which copies every bound `UIconfig.value` into its `Configurable.BoxedValue` in one batched pass --
and that method only runs on this screen's native Apply/Exit lifecycle, not per click. Every OTHER
write on this screen (`SetEnabledFor`, `SelectMode`) already sidesteps this entirely because they're
driven by plain buttons that set the `Configurable` directly in code and immediately force a save
(`SaveConfigNow`, see its own doc comment on Gotcha-adjacent history) -- they never go through a
`UIconfig`'s deferred value plumbing at all. The character-select combo box was the first control on
this screen that actually IS a live `UIconfig` bound straight to a `Configurable`, and it never got
the same immediate-write treatment.

**Fix:** subscribe to `OpComboBox.OnValueChanged` (a genuine per-control event on `UIconfig`,
distinct from `Configurable.OnChange` -- confirmed via decompilation to fire immediately once the
dropdown list closes after a click, not deferred to the next save pass) and, inside it, write
`_selectedCharacterConfig.Value = value` directly (which fires `Configurable.OnChange` ->
`OnCharacterSelectionChanged` -> `RenderDetail()` immediately, since `Configurable<T>.Value`'s own
setter invokes `OnChange` synchronously whenever the value actually changes) followed by
`SaveConfigNow(...)`, matching every other write on this screen. Subscribed once, inside the same
`if (_characterSelectBox == null)` lazy-build block the crash fix above already added, so it's not
re-subscribed (and double-fired) on every `ShowNormal()` call.

**Lesson for any future live Remix control that binds an `OpCheckBox`/`OpComboBox`/etc. straight to
a `Configurable` on this screen:** don't assume the binding alone makes `Configurable.OnChange` fire
on interaction -- it doesn't, unless something forces `_SaveConfigFile()` or writes the Configurable
directly. Hook the control's own `OnValueChanged`/`OnClick` and push the value through by hand, the
same way this fix and `SetEnabledFor`/`SelectMode` already do.

Not yet re-tested in-game -- rebuilt clean and redeployed.

## 2026-09-17 (same day, later still): does Remix support real modal/floating windows? -- no, confirmed again by decompilation

User question, separate from the bugs above: can the Remix config screen host an actual popup
window/modal of any kind, rather than the in-tab overlay approach `PrimalOptions`'s own header
comment already describes choosing? Re-confirmed by decompiling `Menu/Dialog.cs`: `Menu.Dialog` is a
**completely separate `ProcessManager.Process`** (its own `ProcessID.Dialog`, a full-screen darkening
sprite plus its own `Page`/`MenuObject` tree) -- structurally unrelated to `OptionInterface`/
`ConfigContainer`/`OpTab`, and built from an entirely different, lower-level vocabulary
(`Menu.MenuLabel`, `Menu.SimpleButton`, `RoundedRect`, raw `FSprite`s) that knows nothing about
`UIconfig`/`Configurable` at all. Every control this mod's settings screens are built from
(`OpCheckBox`, `OpComboBox`, `OpScrollBox`, `OpKeyBinder`, ...) only exists as a child of a
`ConfigContainer`'s `OpTab` -- none of it can be parented into a `Menu.Dialog`, and there is no
bridge between the two systems. So: **no**, Remix's own config UI genuinely does not support a true
floating window for anything built the way this mod's screens are built. The only two options are
(1) the in-tab overlay this mod already uses (swap `_detailItems`, draw a backdrop `OpRect` behind
it, still literally part of the same flat tab) or (2) rewrite the specific screen from scratch in
`Menu.Dialog`'s own vocabulary (the way `CommunityChallengesDialog`/`KillListDialog` already do
elsewhere in this mod) -- which means giving up every existing `Op*` control and reimplementing
checkboxes/scrollboxes/comboboxes by hand in that vocabulary, a full rewrite, not a small change.

## 2026-09-17 (same day, later still): Remix menu redesign round 3 -- character grid is back, Change Mode/slot-switch merged, Add buttons moved up, scrollbar clearance fixed

Requested directly, from an annotated screenshot after the first in-game look at round 2's layout:
(1) replace the character OpComboBox with dedicated per-character buttons again, colored per
character, showing only the character's name; (2) fit that whole grid into the same top-left region
the title/mode/slot-switch text used to occupy; (3) the top-right region should be just ONE
"Change Mode" button with its label split across two lines, and the slot-switch-key rebinder should
be removed entirely; (4) move "+ Add Gimmick"/"+ Add Requirement" to the top of their columns and
drop the "GIMMICKS"/"REQUIREMENTS" captions; (5) fix list rows visibly running into the OpScrollBox's
own slide bar.

**Character grid, again -- but this time colored, and this time safe to rebuild.** The round-2
OpComboBox is gone; `BuildPermanentItems()` now builds one `OpSimpleButton` per `CharacterEntry`
(5 columns, row-major), each tinted via the existing `GetHomeCharacterColor` (previously only used
for gimmick/requirement row icons, now also called with a CHARACTER's own `SlugcatStats.Name` --
`CharacterEntry` gained a `Name` field alongside `ID`/`DisplayName` specifically so this call has
something to pass). Button text is just `entry.DisplayName`, per the request -- no "PRIMAL " prefix
or description, unlike the old pre-OpComboBox grid design or the character-select-screen labels
elsewhere in this mod. Each button's `OnClick` calls a new `SelectCharacter(id)` that writes
`_selectedCharacterConfig.Value` directly and force-saves (`SaveConfigNow`) -- exactly the
"OnClick writes the Configurable and saves immediately" pattern `SetEnabledFor`/`SelectMode` already
established, so this sidesteps the OpComboBox's whole deferred-write problem class by construction
rather than needing another live-update workaround. **Unlike the OpComboBox, these buttons do NOT
need the Gotcha #4 caching treatment** -- plain `OpSimpleButton`s are not `UIconfig`/`Configurable`-
bound controls, so rebuilding them fresh on every `BuildPermanentItems()` call (which happens on
every ShowNormal(), i.e. every ModeSelect->Normal transition) is safe; only controls that actually
wrap a `Configurable<T>` (OpCheckBox, OpComboBox, OpKeyBinder, ...) have that restriction.

**Change Mode + slot-switch-key merged into one button; the key is no longer rebindable from this
screen.** `ChangeModeButtonWidth`/`Height` size one button to occupy the exact combined footprint
the two old stacked buttons used, with its label set to the literal string `"Change\nMode"` --
relies on FLabel (the underlying Futile label every Op* text element renders through) honoring a
literal newline as a line break, which is a real assumption worth checking on the next actual build,
flagged inline in the constants' own comment since this is the first place in this codebase that's
needed multi-line button text. `PrimalSettings.BuildSlotSwitchBinder`/`OpKeyBinder` are simply no
longer called from anywhere in `PrimalOptions.cs` -- left in place in `PrimalSettings.cs` (not
deleted) since `PrimalSettings.SlotSwitchKey` is still read every frame by
`PrimalSlotTabSwitchHook.cs` for the actual slot-switch keypress detection, completely independent
of whether a UI control exists to rebind it. **Known, accepted regression: the slot-switch key is
now stuck at whatever was last persisted (default: Tab) with no in-game way to change it** -- this
was requested explicitly ("the switch button select should be removed"), not an oversight; if a way
to rebind it is wanted later, `BuildSlotSwitchBinder` is still there to call from wherever it should
live next.

**Add buttons moved to the top; header captions dropped.** In `RenderSplit()`, `addGimmickButton`/
`addRequirementButton` are now built and added to `items` BEFORE their corresponding
`OpScrollBox`, and now sit at `ColumnAddButtonY = DetailTopY - ColumnAddButtonHeight` (top of the
column) rather than `DetailBottomY` (the old bottom position) -- the `"GIMMICKS"`/`"REQUIREMENTS"`
`OpLabel`s that used to sit above them are gone outright, not just repositioned. `ColumnListViewportTop`/
`Bottom`/`Height` were recomputed accordingly: the scrollable list now fills essentially the whole
rest of the column, from just below the Add button down to the very bottom of the detail area (no
more space reserved at the bottom for a button that isn't there anymore).

**Scrollbar-clearance fix.** Root cause, confirmed by decompiling `OpScrollBox`: a vertical scroll
box draws its own slide bar inset from ITS OWN right edge (`size.x - 20f`), but nothing about
`AddItems` accounts for that reservation -- and `ColumnNameButtonW` was originally defined so a
row's icon + name button + gap + "x" remove button added up to EXACTLY `ColumnWidth`, meaning the
remove button's right edge sat flush against, and visually under, the slide bar. New constant
`ColumnScrollbarClearance = 24f` is subtracted up front in `ColumnNameButtonW`'s own formula, so
every row now stops short of the column's true right edge by that much, mirroring the exact
reasoning `ModalListViewportWidth = ContentWidth - 40f` already used for the wider modal scroll
boxes elsewhere in this same file. This only affects `RenderSplit()`'s own two-column list rows --
the Add*Modal pickers already had equivalent clearance built in from when they were first written.

**Layout math, for anyone adjusting these constants further:** with `ContentWidth = 580`,
`ChangeModeButtonWidth = 140` and a 5-column grid, each character button works out to roughly 78px
wide -- noticeably narrower than the pre-OpComboBox grid design had (which used nearly the full
content width). Longer display names ("Spearmaster", "Artificer") will very likely get
ellipsis-trimmed by `OpSimpleButton`'s own `LabelTest.TrimText` at that width -- ugly but not broken
(no overflow, no crash) -- worth a look on the next real build and possibly worth widening the grid
area (at the cost of the Change Mode button's own width) if it reads badly in practice. All of this
was reasoned from the decompiled UI toolkit and existing layout constants, not from an in-game
screenshot of the new layout -- **not yet built-and-seen in-game**, only compiled clean.

## 2026-09-17 (same day, later still): Add-picker category tabs, general-requirement renames, UniqueKills split off from Artificer, and a second scrollbar-overlap fix

Requested directly, from two annotated screenshots of round 3's in-game look (a working "Add
Gimmick" picker, and an "Add Requirement" picker whose slide bar was visibly drawn over its own 4th
column): (1) split the AddGimmick picker into General (no tied character) / Specialized (tied)
category tabs; (2) split AddRequirement into General / Community Challenges tabs; (3) stop printing
a character's name in a General requirement's own title; (4) split the Artificer combo requirement
into a "Kill List" half and a "more generalized, not slugcat specific" Ascend half; (5) three
renames (Inv -> "Highest Point", Spearmaster -> "Quick Broadcast", Artificer -> "Unique Kills"); (6)
actually fix the scrollbar overlap this time.

**Category tabs, derived from existing data, no new fields.** Both categorizations turned out to
already be fully computable from what `GimmickDef`/`Requirement` already expose: "General" for a
gimmick means `HomeCharacter == null` (Kill List, No Passages -- exactly the two examples the
request itself gave); "General" for a requirement means it's NOT one of the 50 curated IDs in
`CommunityChallengesRoster` (the same `.Contains` check `GetRequirementIcon` already uses for its
own white/slugpup tag). `RenderAddGimmickModal`/`RenderAddRequirementModal` each gained two new
full-width `OpSimpleButton`s (`PickerCategoryTabWidth = (ModalListViewportWidth -
PickerCategoryTabGap) / 2`) between the "< Back"/title row and the grid, filtering `candidates`
before the grid is built; clicking a tab sets a new per-modal enum field
(`_gimmickPickerCategory`/`_requirementPickerCategory`, reset to General every time "+ Add" opens
the picker fresh) and re-renders the whole modal, the same "click -> rebuild the detail area"
interaction every other control on this screen already uses. The active tab is tinted white
(`colorEdge = Color.white`) so it's visually obvious which one is showing. `PickerListViewportTop`/
`Height` (new) shrink the grid's own viewport to make room for the tab row above it --
`ModalListViewportTop`/`Height` are unchanged and still used by the (tab-less) GimmickSettings modal.

**General requirements no longer print the tied character's name, and three got renamed.** Per
direct request, dropped the "X -- " prefix from the three still-character-tied requirements'
`DisplayName`s (their row's own icon+color already carries that, matching how gimmick rows have
always worked) and renamed them at the same time: `GourmandFoodQuestRequirement` -> "Food Quest &
Save" (name unchanged, just de-prefixed), `SpearmasterFastEndingRequirement` -> "Quick Broadcast",
`InvHighestPointRequirement` -> "Highest Point".

**ArtificerKillAscendRequirement split apart; its kill-tracking half is now UniqueKillsRequirement,
fully general.** Per direct request ("not slugcat specific... kill list and... ascend" as two
separate things): the "ascend" half is dropped outright, not reimplemented anywhere -- it was always
a pure duplicate of the already-existing, already-general `AscensionRequirement` (one of the 50
curated Community Challenges), which reads the exact same `standardVoidSea` flag. What's left, "kill
every Kill List species" (`HasKilledEveryKillListSpecies`, unchanged logic, moved verbatim), is now
`UniqueKillsRequirement` -- no `HomeCharacter`, no per-character gate, checked against
`TryGetEnabledCharacter` alone so it can be enabled for, and completed by, any character. Since it's
no longer gated to a game-ending event, it needed a genuinely new trigger: checked live the instant
the last missing species is credited, via `PrimalKillTracker.OnKillCredited`, which **gained a
`Player` parameter** (`System.Action<string, Player>`, was `System.Action<string>`) specifically for
this -- `PrimalKillDieHook`'s own Postfix already resolves a `Player` right before raising the event,
it just wasn't threaded through before. The one existing subscriber (`SaintRequirement`, "any kill
disqualifies") was updated to ignore the new parameter (`(characterID, _) => ...`) rather than
needing it. Registered as a new "general" entry in `RequirementRegistry.All`, separate from the
three still-character-specific ones above it (with a comment explaining the split, so a future
reader doesn't go looking for a fourth character-specific class that no longer exists).

**Scrollbar overlap, round 2 -- the round-3 fix didn't actually apply to the Add*Modal pickers.**
Re-diagnosed from the screenshot rather than assumed fixed: `ModalListViewportWidth = ContentWidth -
40f`'s own comment claimed it already left "clearance... for the scroll box's own slide bar," but
that's not what it does -- it only makes the scroll box itself narrower than the full screen width;
`PickerColumnWidth` was still sized to fill that ENTIRE width, so the last column's buttons landed
exactly on top of where `OpScrollBox` draws its slide bar (`size.x - 20f`, same decompiled fact
`ColumnScrollbarClearance` already relied on). Fixed the same way: a new `PickerScrollbarClearance`
subtracted before the per-column math (`PickerContentWidth = ModalListViewportWidth -
PickerScrollbarClearance`), leaving the OpScrollBox's own width/backdrop untouched. **Also bumped
both `ColumnScrollbarClearance` and this new `PickerScrollbarClearance` from 24f to 30f** -- the
math at 24f left only a ~4px gap before the slide bar's own confirmed 20px width, uncomfortably
thin for one more rounding adjustment somewhere else in either row's layout to eat into; 30f leaves
a more comfortable margin.

**Not yet built-and-seen in-game** -- compiled clean and redeployed, but this round's actual pixel
layout (especially "Community Challenges" fitting in a ~265px-wide tab button, and whether 30f is
enough clearance in practice, not just on paper) hasn't been looked at yet.

## 2026-09-17 (same day, later still): the per-row icons removed entirely -- color-only again

Requested directly: "remove the slugpup icons from all gimmicks and requirements." Every icon this
screen ever drew (gimmick rows, requirement rows, both Add*Modal pickers) was, per the same-day
"character icons added" section above, always some tint of the single "Kill_Slugcat" sprite --
including the Community Challenges' own "slugpup" tag (`MoreSlugcatsEnums.CreatureTemplateType
.SlugNPC` also resolves to "Kill_Slugcat") -- so "the slugpup icons" meant all of them, not just the
Community Challenges' own white one.

**Removed outright, not just stopped calling:** `CharacterIconSprite()`, `CommunityIconSprite()`,
the `RowIcon` struct, `GetGimmickIcon`/`GetRequirementIcon`, and `BuildIconElement` are all gone from
`PrimalOptions.cs`. `RowIconSize`/`RowIconGap` (the layout constants that reserved room for an icon
beside each row/picker-entry) are gone too, and `ColumnNameButtonW`'s own formula no longer
subtracts them -- name buttons/labels get that width back.

**Coloring is preserved -- it was never really about the icon.** `GetHomeCharacterColor` (unchanged)
is now called directly at each of the four call sites (gimmick rows, requirement rows, both pickers)
instead of being wrapped in a `RowIcon`. Community Challenges keep their plain white tint via a new,
much smaller `GetRequirementColor(Requirement)` (white if `CommunityChallengesRoster.Contains(id)`,
else `GetHomeCharacterColor(requirement.HomeCharacter)`) -- replacing `GetRequirementIcon` one to
one, just returning `Color?` instead of `RowIcon?`. Every row/picker-entry's `colorEdge` (or, for a
requirement's plain label, `.color`) is set exactly as before; only the little image beside it is
gone. This is, functionally, exactly the color-only look this screen had before the 2026-09-17
"character icons added" pass introduced icons in the first place.

**Not yet seen in-game** -- compiled clean and redeployed.

## 2026-09-17 (same day, later still): requirement rows' button look restored, screen-wide padding bumped, hover bump-highlight no longer pokes out of bounds

Requested directly, after looking at the icon-removal pass: "requirements lost their button
appearance and hovering over a gimmick makes it go a bit out of bounds -- add a bit more left and
right padding to this window and the gimmick and requirements windows."

**Requirement rows are OpSimpleButtons again (greyedOut, not clickable).** They were deliberately
made a plain `OpLabel` in UI redesign round 2 ("there's no requirement-settings screen to open yet
... a button that opens nothing would just be a dead click target") -- that reasoning still holds,
so this isn't reverted to a real clickable button. Instead, `RenderSplit`'s requirement row is now an
`OpSimpleButton` with `greyedOut = true` set unconditionally (not tied to `IsReadOnly` -- there's
still nothing to click regardless of mode). Confirmed via decompiling `OpSimpleButton.GrafUpdate`
that a greyed-out button still draws its bordered `_rect` (just without the hover/focus highlight
ring, and its click-handling branch never runs) -- exactly "looks like a button, reads as
non-interactive," restoring the visual weight/border a plain label didn't have, without reopening
the "dead click target" concern.

**"Hovering over a gimmick makes it go a bit out of bounds" -- root cause, confirmed by
decompilation, not guessed: `OpSimpleButton`'s own hover highlight (`_rectH`) grows a few px beyond
the button's own rect on focus/hover (`GrafUpdate`'s `_rect.addSize = new Vector2(6f, 6f) *
bumpBehav.AddSize`).** A button built flush to its container's own edge (every gimmick/requirement
row's name button sat at LOCAL x=0 within its OpScrollBox, and the Split view's Add buttons/scroll
boxes sat flush at `LeftColumnX`/`RightColumnX`) has nowhere for that growth to go but past the
visible boundary. Fixed with a new `ColumnSidePadding` (8f) that insets the ENTIRE column's content
-- the Add button and the OpScrollBox itself, not just individual rows -- from both
`LeftColumnX`/`RightColumnX`, shrinking their effective width to a new `ColumnContentWidth`
(`ColumnNameButtonW`'s own formula was rebased onto this instead of the raw `ColumnWidth`). Since
row content coordinates are LOCAL to the (now inset) scroll box, this fixes the hover-bump issue for
every row automatically without touching the per-row layout math at all. The exact same treatment
(`PickerSidePadding`, `PickerAreaX`/`PickerAreaWidth`) was applied to the AddGimmick/AddRequirement
pickers' own category tabs and scroll box, for the same reason and the same fix shape.

**Screen-wide padding.** `ContentLeft`/`ContentWidth` (10f/580f, i.e. a 10px margin on each side of
the ~600-wide screen) became 18f/564f -- an extra 8px of margin on both the left and right edges of
the WHOLE screen, per the "add a bit more left and right padding to this window" half of the
request. Since virtually everything on this screen derives its X position from these two constants,
this one change shifts the title, character grid, Change Mode button, both Split columns, and every
modal's own frame inward together, keeping all of them proportionally consistent with each other
without needing separate adjustments.

**Not yet seen in-game** -- compiled clean and redeployed, but the exact pixel result (especially
whether `ColumnNameButtonW`/`PickerColumnWidth`, both now a bit narrower than before these paddings
were added, still fit the longest gimmick/requirement display names without clipping) hasn't been
looked at yet.

## 2026-09-17 (same day, later still): the greyedOut fix was wrong -- it forces grayscale, not just "no hover ring"; padding still wasn't enough

User reported back, with screenshots, on the previous round's two fixes: (1) requirement rows now
render visibly grey/washed-out ("requirements now look greyed out" -- request: "just make them look
like gimmicks"), and (2) hovering a gimmick's own name button still visibly pokes past its column's
edge ("you didnt fix the out of bounds button anywhere").

**Root cause of (1), confirmed by decompiling `BumpBehaviour.GetColor`, not re-guessed:**
`greyedOut` doesn't just skip the hover ring the way the previous pass assumed -- when true,
`GetColor` unconditionally returns `MenuColorEffect.Greyscale(MenuColorEffect.MidToVeryDark(orig))`
instead of the normal color-lerp path, for BOTH the border (`_rect.colorEdge`) and the label
(`_label.color`, which is also routed through `GetColor`). So every requirement row -- colored or
not -- rendered desaturated and darkened, exactly matching the screenshots (a Community-Challenge
row that should have been plain white came out dim grey; a character-colored one came out a muddy,
washed-out version of its real color). **Fixed by dropping `greyedOut` entirely.** Requirement rows
are now a plain `OpSimpleButton` with no `OnClick` subscriber at all -- per direct instruction ("just
make them look like gimmicks") they now render with full, normal color and normal hover/focus,
identical to a gimmick's own name button. Confirmed via decompiling `OpSimpleButton.Update()` that
clicking still does nothing when `OnClick` is null: the click branch is `if (this.OnClick != null) {
this.OnClick(this); PlaySound(soundClick); }`, so no sound plays and nothing happens -- there is no
`greyedOut`-shaped "looks disabled" cue, but there's also no dead-click confusion (the original round
2 concern) since it's silent.

**(2) was a real miss, not a misreading -- 8f of padding wasn't enough.** `ColumnSidePadding`/
`PickerSidePadding` (added the previous round specifically for this) went from 8f to 16f. Re-reading
`BumpBehaviour.AddSize`'s own formula more carefully: it's not a flat "+1 while focused" -- `sizeBump`
approaches 1 via `LerpAndTick`, PLUS a transient `0.5 * Mathf.Sin(extraSizeBump * pi)` term that
peaks mid-ramp (around `extraSizeBump == 0.5`) while `sizeBump` is already substantial, so the
combined value can exceed 1 for a moment during the focus animation, not just settle at a flat
ceiling -- `_rect.addSize = new Vector2(6f, 6f) * AddSize` (confirmed via `DyeableRect`: `addSize`
expands the rect symmetrically, offset by `-addSize * 0.5`) can therefore momentarily grow more than
the "obviously safe" reading of the formula suggested. 16f is a deliberately generous margin rather
than a tightly computed exact minimum, given this is the second miss in a row on this exact
calculation.

**Not yet seen in-game** -- compiled clean and redeployed. If 16f still isn't enough on the next
look, the next step should be measuring the actual on-screen pixel overflow directly (a screenshot
with a ruler/grid, or reading `AddSize`'s live value via a temporary debug log) rather than a third
attempt at computing it from the formula alone.

## 2026-09-17 (same day, later still): the padding was inset on the wrong element -- container moved instead of the child padded within it

User correction, with screenshots, on the previous round's padding fix: "youre adding padding to
the CONTAINER... look how much empty space there is now from the edge of the window to where the
button is, but thats just the gap between the container - the buttons edge on hover still goes out
of bounds."

**What the previous pass actually did wrong, once stated plainly: it moved the CONTAINER (the
OpScrollBox, and the Add button) inward from the column's own edges, rather than keeping the
container where it already was and padding the CHILD drawn inside it.** Those are not the same
operation. Moving the container creates a visible dead gap between the column's true edge (where
the character grid above, and everything else on this screen, still lines up) and the relocated
container -- while a row/button still drawn edge-to-edge WITHIN that relocated container has exactly
as much clearance from ITS OWN immediate bounds as before (none), so it still overflows on hover,
just against a boundary that moved and is now harder to see relative to everything else.

**Fixed by reverting every container to its original position/size and padding only the child drawn
inside it:**
- Both Split-view `OpScrollBox`es are back to the column's full, original `(LeftColumnX/
  RightColumnX, ColumnWidth)` extent. `ColumnNameButtonW`'s formula now subtracts `ColumnSidePadding`
  directly (alongside the existing `ColumnScrollbarClearance` on the right) from the full
  `ColumnWidth`, and each row's name button (plus the "No gimmicks/requirements..." empty-state
  label) is positioned at local `x = ColumnSidePadding` instead of `0f` -- genuine clearance from the
  scroll box's own true left edge, without moving that edge.
- The "+ Add Gimmick"/"+ Add Requirement" buttons reverted to the column's own unpadded extent
  entirely (`LeftColumnX`/`RightColumnX`, full `ColumnWidth`) -- matching the character grid's own
  alignment above them. The reported hover-overflow was specifically about gimmick/requirement ROWS
  (e.g. "hovering over a gimmick"), never the Add button itself, so it needed no inset at all; giving
  it one only produced the exact misaligned-gap artifact the screenshots called out.
- Same treatment for the Add*Modal pickers: both scroll boxes are back to the modal's full
  `(ContentLeft, ModalListViewportWidth)` extent, `PickerContentWidth` now subtracts
  `PickerSidePadding` (left) and `PickerScrollbarClearance` (right) from that full width directly,
  and each grid entry's `x` gets a `PickerSidePadding` head-start instead of starting at `0f`. The
  category tabs (General/Specialized, General/Community Challenges) reverted to the same unpadded
  `(ContentLeft, ModalListViewportWidth)` extent as the "< Back" button and title label directly
  above them, for the same alignment reason the Add buttons did -- `PickerAreaX`/`PickerAreaWidth`
  (the constants that inset them) are gone entirely, not just unused.

**Lesson, worth remembering for any future padding/inset request on this screen:** "add padding
around X" means X gets smaller/repositioned within an UNCHANGED parent, not "move X's parent inward
and leave X exactly as big as its new parent." The two look superficially similar in the constants
(both involve subtracting a padding value somewhere) but produce visibly different, and differently
broken, results.

**Not yet seen in-game** -- compiled clean and redeployed.

## 2026-09-17 (same day, later still): padding mechanism confirmed correct in-game; 16f dialed back to 8f

User confirmed, with screenshots, that padding the row (child) within an unmoved scroll box
(container) -- the previous round's fix -- actually works: no more misaligned dead-space gap, and no
more hover overflow. Only complaint: 16f is "too much" now that it's applied correctly.
`ColumnSidePadding`/`PickerSidePadding` both dialed back to 8f. Worth noting for anyone reading the
history here: the EARLIER "8f wasn't enough" result (the round before this one) was measured while
the padding was still being applied to the wrong element entirely (the container, not the row) --
that test never actually exercised what 8f alone does once padding is on the right thing, so
reverting to 8f now isn't repeating a known-bad value, it's the first fair test of it.

Not yet seen in-game -- compiled clean and redeployed.

## 2026-09-17 (same day, later still): Change Mode narrowed, character grid buttons enlarged

Requested directly, after confirming the padding fix looked right in-game: "reduce width of change
mode and increase the size of character buttons." `ChangeModeButtonWidth` 140f -> 100f;
`CharGridButtonHeight` 32f -> 40f. `CharGridButtonWidth` needed no direct edit -- it's already
derived from the leftover space next to the Change Mode button (`CharGridAreaWidth = 
ChangeModeButtonX - ContentLeft - 20f`), so narrowing that button alone widens every grid button too
(roughly 74px -> 82px). `ChangeModeButtonHeight` was a separate literal (72f) that happened to equal
the grid's old total height (`32*2 + 8`) -- turned into a real derived constant
(`CharGridButtonHeight * 2f + CharGridRowGap`) instead of a second literal that could drift out of
sync the next time either value changes, since keeping the two in visual lockstep is the whole point
of `ChangeModeButtonY = CharGridBottomY` right below it. Moved the CharGrid* constants block above
the title-row block since `ChangeModeButtonHeight` now references `CharGridButtonHeight` (C#
resolves const-to-const references across the whole class regardless of textual order, so this
reordering is purely for readability, not required for it to compile).

Not yet seen in-game -- compiled clean and redeployed.

## 2026-09-18: Saint gameplay mechanics, round 1 -- No Hold, Rain Timer, and Blizzard Timer

New session, continuing past the 2026-09-17 UI-polish batch above. User asked to work through "the
rest of the gameplay altering mechanics before testing" and started with three items scoped to
Saint: No Hold (generalizing No Spears), Rain Timer, and Blizzard Timer. Also confirmed this
session's own container has no `.NET` toolchain (same limitation documented throughout this file's
older "Container environment notes" section below -- the 2026-09-15 "sessions now run on the user's
own Windows machine" note near the top of this doc does NOT hold for this particular session; check
`dotnet --version` fresh each time rather than trusting that note).

**All three are now implemented** — see their own file-map bullets above (`NoHoldGimmick.cs`,
`RainTimerGimmick.cs`, `BlizzardTimerGimmick.cs`) for the full account of each. None has been built
or tested yet (no toolchain here). Short summary:
- **No Hold** replaces `NoSpearsGimmick.cs` outright (deleted) with a 10-category picker (Spears,
  Rocks, Bombs, Lanterns, Fruits, Batflies, Squidcadas, Yeeks, Centipedes, Other), reusing the exact
  `Player.Grabability` Postfix mechanism every other No* gimmick already used. `GimmickDefaults.cs`
  updated in place (Spearmaster/Inv's old `NoSpears` defaults now point at `NoHold`, unchanged
  behavior). `GimmickRegistry.cs`, `PrimalOptions.cs`, `ChainsGimmick.cs`, `NoYeeksGimmick.cs`,
  `NoCicadasGimmick.cs` all had small comment-only updates to stop referencing the now-deleted class.
- **Rain Timer** rescales `RainCycle.cycleLength`/`sunDownStartTime` by a configurable percentage,
  via a Postfix on `RainCycle`'s constructor -- confirmed to control Saint's blizzard automatically
  (same underlying timer, see the file-map bullet for the full trace) with a hard floor specifically
  designed around a real vanilla math bug the user flagged from a prior non-mod attempt (see that
  bullet for the exact derivation of the ~3220-tick breaking point in
  `MoreSlugcats.BlizzardGraphics.CycleUpdate`). No default assignment to Saint yet -- ask/wait for a
  starting percentage rather than guessing one.

- **Blizzard Timer** — mechanism researched first, then the user was asked which unit to expose
  (per their own explicit instruction: "let me know first how the timer works and then ask me"),
  and answered **"Total minutes into the cycle."** Implemented as a Postfix on
  `RainWorldGame.BlizzardHardEndTimer` (its one and only call site anywhere in the game) that
  overrides the hardcoded post-rain delay to whatever makes `cycleLength + result` equal the
  configured total-cycle-minute mark, via a stashed-current-creature handoff from a second Prefix/
  Postfix pair on `Creature.HypothermiaUpdate` (see the file-map bullet and this gimmick's own
  header comment for exactly why that indirection is needed, and why `cycleLength` itself was
  deliberately NOT touched directly). Full mechanism trace, preserved below for reference:
```
float num3 = (float)room.world.rainCycle.cycleLength + (float)RainWorldGame.BlizzardHardEndTimer(room.game.IsStorySession);
HypothermiaGain = Mathf.Lerp(0f, RainWorldGame.DefaultHeatSourceWarmth * 0.1f, Mathf.InverseLerp(0.1f, 0.95f, room.world.rainCycle.CycleProgression));
HypothermiaGain += Mathf.Lerp(0f, RainWorldGame.BlizzardMaxColdness, Mathf.InverseLerp(0f, num3, room.world.rainCycle.timer));
HypothermiaGain += Mathf.Lerp(0f, 50f, Mathf.InverseLerp(num3, num3 * 5f, room.world.rainCycle.timer));
...
HypothermiaGain = Mathf.Clamp(HypothermiaGain, -1f, 0.0055f);
```
`RainWorldGame.BlizzardHardEndTimer(storyMode)` returns a hardcoded **32000 ticks** (800 real
seconds, 13m20s) in story mode. `num3 = cycleLength + 32000` is the real "harshening" threshold:
below it, per-tick Hypothermia gain ramps gently (capped near `BlizzardMaxColdness ≈ 0.0016`/tick);
the instant `room.world.rainCycle.timer` (which keeps counting up even *after* rain/blizzard has
already hit -- `RainCycle.Update()`'s `timer++` is unconditional, not gated on `timer < cycleLength`)
crosses `num3`, the third line's `+50` term dwarfs everything else and the whole expression
immediately saturates at its hard ceiling, `0.0055`/tick -- i.e. an effectively-instant jump from
"gentle background cold" to "the maximum possible cold gain the game allows," exactly matching the
user's own description ("suddenly gets significantly harsher"). Since `timer` only starts counting
toward this AFTER `cycleLength` ticks have already passed (rain/blizzard onset), the real
wall-clock delay the player experiences is `cycleLength + 32000 ticks` measured from cycle START,
which for a plausible Saint cycle length comfortably lands in the user's own observed "20-30
minutes" window -- this is almost certainly the exact mechanic being described, not a guess.

**Resolved:** the user picked "Total minutes into the cycle" over the alternatives offered (raw
post-onset minutes/ticks matching `TimeLimitSettings`' own convention, or a percent-of-vanilla
slider matching Rain Timer's own style) -- see `BlizzardTimerSettings` in
`BlizzardTimerGimmick.cs` for the resulting single `Configurable<int>` (default 25 minutes).

## 2026-09-18 (later): Rain Timer's blizzard-render bug actually fixed, not just floored around; Saint gets real defaults

Follow-up, same session. User asked two things: (1) look into the ~3220-tick "blizzard doesn't
render" bug Rain Timer's first pass only floored around, and actually fix it if possible; (2) give
Saint real default values -- Rain Timer at 0% ("it needs to start immediately") and Blizzard Timer
at 60 minutes.

**The render bug is now fixed at its actual source**, not just avoided. Re-derived the exact
breakdown by hand (not just re-stating the earlier "13%" finding): `BlizzardGraphics.CycleUpdate`'s
"glacial wasteland" branch (Saint's own home region's normal ambient blizzard, as opposed to the
separate `AerieBlizzard`-flagged-room case, which was never at risk -- it uses fixed absolute-tick
thresholds, not `cycleLength`-relative ones) computes
`BlizzardIntensity = Mathf.InverseLerp(cycleLength * 0.932f, 3000f, TimeUntilRain)`. Worked the
actual arithmetic for `cycleLength` under ~3220 ticks (3000/0.932): the ramp doesn't just clamp to a
flat 0 -- `Mathf.InverseLerp` still returns a valid fraction with the bounds inverted, so the ramp
runs BACKWARDS -- a small nonzero intensity right at cycle START, collapsing to a clamped 0 well
BEFORE rain/blizzard actually hits. That's the real mechanism behind "the blizzard not visually
appearing": it's not invisible the whole cycle, it fades out exactly when it should be most intense.

**Fix:** `BlizzardVisualCycleLengthHook` (new, same file as `RainTimerGimmick`, `RainTimerGimmick.cs`)
-- a *separate*, *unconditional* Harmony patch (Prefix+Postfix) directly on
`BlizzardGraphics.CycleUpdate` itself, not gated on any gimmick or PrimalSlot. It temporarily
substitutes a safe floor (4200 ticks, the same margin-above-3220 value the old gameplay floor used
to be) for `RainCycle.cycleLength` for the exact duration of one `CycleUpdate` call -- confirmed to
be the ONLY method in the whole game that ever reads `cycleLength` for a rendering purpose -- then
restores the real value immediately after. Because this is a correctness fix for a genuine vanilla
bug rather than a gimmick effect, it deliberately has no gate at all: it only ever changes anything
once `cycleLength` is already abnormally short, which never happens in untouched vanilla, so it's
inert everywhere except when this mod's own Rain Timer (or, in principle, any other mod) has pushed
the timer that low.

With the actual bug fixed, `RainTimerGimmick`'s own gameplay-side floor
(`RainTimerGimmick.MinCycleLengthTicks`) could be dropped from the old 4200 down to 40 ticks (1
second) -- now purely an anti-division-by-zero guard (`RainCycle.CycleProgression`, `AmountLeft`,
etc. all divide directly by `cycleLength`; a literal 0 would produce NaN/Infinity there), not a
render-bug guard anymore. `RainTimerSettings.MinPercent` correspondingly dropped from 10 to 0, so
"0%" is now a reachable, safe config value that clamps to that 1-second floor rather than a true
instant zero.

**Saint's real defaults**, in `GimmickDefaults.cs`: Rain Timer and Blizzard Timer are now both
assigned to Saint by default (`GimmickAssignments.SetDefault(..., true)` for both IDs). The actual
VALUES (0% / 60 minutes) aren't something `SetDefault` itself carries -- each gimmick's own settings
class special-cases Saint's default inside its own `Bind()` (`RainTimerSettings.SaintDefaultPercent
= 0`, `BlizzardTimerSettings.SaintDefaultMinutes = 60`), the same per-character-default pattern
introduced for this. Every other character still gets each gimmick's generic default (100% / 25
min) if ever assigned it manually via Remix.

**Not yet built or tested as of this doc** (no `.NET` toolchain in this container). When retesting,
in addition to Rain/Blizzard Timer's own existing checklists (see their file-map bullets above):
(1) confirm Saint's Remix panel shows Rain Timer and Blizzard Timer both already assigned, at 0%
and 60 min respectively, on a fresh save; (2) confirm rain/blizzard onset is now near-instant for
Saint at the 0% default, AND the blizzard still visually renders (this is the one that would have
failed before today's fix); (3) as an extra check beyond what's already flagged, try Rain Timer at
0% on some OTHER blizzard-affected setup if one exists, to confirm the render fix isn't somehow
Saint-specific despite being written to be character-agnostic.

## 2026-09-18 (later still): "Disable timer" checkboxes for Rain/Blizzard Timer, and a new Precycle Timer gimmick

Follow-up, same session. User asked for two things: (1) a way to make Rain Timer's and Blizzard
Timer's numeric setting mean "unlimited" (originally floated as a special slider notch, then
resolved to a simpler design after a clarifying question -- see below); (2) a brand new gimmick,
Precycle Timer, controlling vanilla's "shelter failure"/precycle mechanic.

**The slider+textbox UI question, and why it was asked rather than guessed:** the user's exact ask
("add a textbox to input the slider value") assumed there wasn't already a way to type a number
directly. Checked the actual decompiled Remix source first rather than assuming either way:
`OpUpdown` (what this mod's "sliders" have always actually been, confirmed via grep -- there's no
`OpSlider` anywhere in this codebase) already extends `OpTextBox` and is directly typable, click and
type a number, no separate box needed. But `Menu.Remix.MixedUI.UIconfig`'s own constructor throws
`MultiuseConfigurableException` if a second UI element ever tries to bind the same `Configurable`
that's already bound to a live one (`config.BoundUIconfig != null` check, confirmed by reading
`UIconfig.cs` directly) -- so a REAL drag-slider (`OpSlider`) next to a genuinely separate typable
box, both pointing at the same literal stored value, isn't possible without a second, manually
synced `Configurable` and reentrancy-guarded `OnChange` wiring on both sides. Given no compiler in
this container to catch a mistake in that sync logic, asked the user to pick between keeping the
already-typable `OpUpdown` (lower risk) or building the real synced pair (more risk, closer to a
literal "slider"). **User's answer superseded both options with a third, simpler one:** keep the
`OpUpdown`, and add a plain `OpCheckBox` to its right labeled "Disable timer" that greys out the
number field and its label when checked.

**Implemented for both `RainTimerGimmick.cs` and `BlizzardTimerGimmick.cs`** (`RainTimerSettings`/
`BlizzardTimerSettings`, same files): a new `Configurable<bool>` per character (`_disabled`),
`IsDisabled(characterID)` getter, and `BuildUI` now constructs the `OpCheckBox` + its own `OpLabel`
alongside the existing `OpUpdown` + label, wiring the checkbox's `OnChange` to set
`opUpdown.greyedOut` (a plain public field on `UIfocusable`, confirmed via the decompiled source --
`OpUpdown` already checks it in its own `Update()`) and swap the numeric label's color to grey (the
label's own *original* color is captured once at construction and restored on uncheck, rather than
hardcoding a "normal" color guess). Checked, each gimmick's own Postfix skips its normal
percent/minutes math entirely and substitutes a large-but-bounded sentinel instead of trying to
truly disable the underlying mechanic (which would mean patching yet another, separate vanilla
method for each): `RainTimerGimmick.UnlimitedCycleLengthTicks` (100 million ticks, ~29 days) is
set directly as `cycleLength` (deliberately WITHOUT also scaling `sunDownStartTime` the way the
normal percent branch does, so day/night keeps behaving like vanilla while only rain/the blizzard
is pushed out to practically-never); `BlizzardHardEndTimerHook.UnlimitedExtraTicks` (also 100
million) is returned directly, independent of `cycleLength`, so it can't overflow when later added
to it in `Creature.HypothermiaUpdate`'s own float-space arithmetic. Saint's own defaults (0% / 60
min from the previous entry) are unaffected -- the new checkbox defaults to unchecked for every
character, including Saint. **Not yet built or tested.**

**New gimmick: Precycle Timer** (`PrecycleTimerGimmick.cs`, new file). Traced vanilla's own precycle
("shelter failure") decision to `RainCycle`'s constructor -- the same method `RainTimerGimmick`
already Postfixes, touching entirely disjoint fields so the two coexist with no ordering dependency.
Two independent vanilla rolls, both now overridden: (1) **whether** a precycle happens --
vanilla rolls `Random.Range(0,100) < region.regionParams.earlyCycleChance * 100` (a PER-REGION
value, not one constant -- matches the user's own "2% in certain regions," but every region defines
its own; Rivulet/Sofanthiel/Watcher get special-cased overrides instead), reseeded deterministically
off the cycle's own seed and saved/restored around the roll so it doesn't disturb any other
`UnityEngine.Random` use in the same constructor. This gimmick re-derives that exact same kind of
roll (same seeding call, `StoryGameSession.SetRandomSeedToCycleSeed(1)`) but compares it against the
character's own flat `FrequencyPercent` (0-100) instead, fully overriding vanilla's decision either
way (0% truly never triggers regardless of the region's own chance; 100% always does). (2) **how
long** the head start lasts if one triggers -- vanilla rolls `Random.Range(4800f, 12000f)` ticks
(120-300 seconds, 2-5 minutes) fresh every time, stored in `maxPreTimer`/`preTimer`; this gimmick
replaces that with a fixed, configured `DurationMinutes` instead. When triggering, sets up the exact
same fields vanilla's own trigger path does (`maxPreTimer`, `preTimer`, the three
`preCycleRainPulse_Wave*` floats, `globalRain.preCycleRainPulse_Scale = 1f`); when not, zeros
`maxPreTimer`/`preTimer` back out, overriding whatever vanilla's own per-region roll (which already
ran earlier in the same constructor call) had already decided. **"Periodic rains" and "different
creatures spawn" needed no new code at all** -- both read `RainCycle.preTimer`/`maxPreTimer` live
wherever they actually happen (confirmed via `preTimer` references in `Room.cs`/`GlobalRain.cs`),
not a separately-cached decision, and this gimmick's Postfix runs well before any room lazily loads
and reads those fields. **One deliberate, flagged scope gap:** vanilla's own SEPARATE "early cycle
flood" roll (`region.regionParams.earlyCycleFloodChance`, gated on `maxPreTimer > 0`) already
consumes its own random value using vanilla's own precycle decision, before this gimmick's Postfix
ever runs -- so a precycle this gimmick forces on (that vanilla itself wouldn't have rolled) never
gets a chance at that flood interaction, and vice versa. Left as-is since flood-on-precycle wasn't
part of what the user asked this gimmick to control; noted here in case a future session is asked to
close that gap too. No default assignment to any character (not requested with a specific value, same
reasoning as Rain/Blizzard Timer before Saint's own defaults were given) -- `homeCharacter: Saint`
for the Remix color tag only, since it was raised in the same Saint-focused conversation. **Not yet
built or tested.**

**Variance question, answered directly from the source rather than guessed (see the chat reply
itself for the full numbers):** vanilla's own `cycleLength`, BEFORE Rain Timer's percentage ever
touches it, already re-rolls every single cycle -- `Lerp(cycleTimeMin, cycleTimeMax, Random.value)`
seconds, defaulting to a 400-800 second (6m40s-13m20s) range that's itself changeable in vanilla's
own Options menu (traced to `World.cs`'s constructor and `RainWorld.cs`'s own default values, lines
1682-1683: `cycleTimeMin = 400; cycleTimeMax = 800;`). Rivulet/Gourmand/Saint get a biased roll
instead (`0.35 + 0.65 * Random.value^1.2`, never sampling the bottom 35% of that range). Rain
Timer's percent multiplies whatever that already-random per-cycle base comes out to, every cycle
independently -- it doesn't replace vanilla's own randomness, it scales it. Precycle's own duration
(`maxPreTimer`) is vanilla's own separate 4800-12000 tick (2-5 minute) roll, rerolled fresh every
time a precycle actually triggers -- now replaced outright by Precycle Timer's fixed
`DurationMinutes` rather than preserving that randomness (a design choice, not forced by anything
technical -- flagged in case the user would rather keep some randomness around a configured
midpoint instead of a perfectly fixed value).

## 2026-09-18 (later still): Precycle Timer gains flood chance + "Force max precycle timer"; region coverage confirmed

Follow-up, same session. User asked for two more `PrecycleTimerGimmick.cs` configs and an explicit
double-check on region coverage.

**Region coverage, checked directly (not assumed):**
- **The timer mechanism itself is region-agnostic, confirmed.** Read `RainCycle.Update()`'s
  precycle gating and `RainWorldGame.AllowRainCounterToTick()` in full -- both key only on
  `ModManager.PrecycleModule`, story/arena session state, ghost-mode cameras, gate transitions, and
  a couple of Watcher/MSC session flags. Zero region-specific code anywhere in that path. So yes:
  Precycle Timer's `FrequencyPercent` at 100 forces a head-start in literally any real playable
  region -- the gimmick's own `world.region == null` guard only excludes non-gameplay single-room
  worlds (menus, dream sequences), never a real region.
- **The flood effect is NOT universally visible, and that's a genuine vanilla limitation, not a gap
  here.** `Room.cs`'s own water-adding code additionally gates on that specific room's
  `RoomRain.DangerType` being `Flood`/`FloodAndRain` -- level-design data baked per-room by each
  region, not something togglable from C#. Setting `drainWorldFlood` still always happens the same
  way vanilla itself does it, but it will only ever produce visible rising water in rooms the
  region's own designers already tagged that way. This is presumably exactly why vanilla's own
  `earlyCycleFloodChance` is per-region in the first place.

**Flood chance (0-100%), new `FloodChancePercent` config.** Vanilla rolls a SEPARATE
`earlyCycleFloodChance` a few lines after the precycle decision, gated on vanilla's OWN
`maxPreTimer > 0` (i.e. whether vanilla's own roll, not this gimmick's, triggered) -- since that
roll already ran using vanilla's decision before this gimmick's Postfix executes, it's re-rolled
independently here too (same seeded-and-restored `UnityEngine.Random` pattern as the trigger roll),
gated on THIS gimmick's own trigger decision instead. On success, sets
`globalRain.drainWorldFlood = 99000f` -- except on the "MS" region (Submerged Superstructure) for
any character other than Rivulet, where it's forced to `0f` instead, faithfully preserving vanilla's
own one exception (MS already manages its own water state; Rivulet alone has some special
interaction with it -- read as a real compatibility guard, not a balance choice, so kept exactly as
vanilla has it rather than folded into the "full override" treatment everything else here gets).

**"Force max precycle timer" checkbox, new `ForceMaxDuration` config.** Modeled directly on
`MMF.cfgNoRandomCycles` (found earlier in `World.cs` -- when checked, that config pins the NORMAL
cycle's own length straight to `cycleTimeMax` instead of rolling between `cycleTimeMin`/`cycleTimeMax`).
Same idea here: checked, a triggered precycle's head start always uses
`PrecycleTimerSettings.MaxDurationMinutes` (999) instead of the configured `DurationMinutes` --
using the same checkbox-greys-out-the-numeric-field UI pattern `RainTimerSettings`'s own "Disable
timer" established two rounds ago (`durationUpdown.greyedOut`, the label's own original color
captured and restored). `PrecycleTimerGimmick.Postfix` picks between the two at the point it
computes `durationTicks`.

All three new UI rows built in `PrecycleTimerSettings.BuildUI` alongside the two already there
(frequency; duration + the new force-max checkbox on one row; flood on a new third row) -- content
still comfortably fits the plain settings viewport, no `settingsContentHeight` override needed.
**Not yet built or tested.**

## 2026-09-18 (later still): Precycle Timer gains Force Submerged Superstructure + Force shelter unavailable

Follow-up, same session. User asked for one more checkbox (skip the MS/Rivulet flood exception for
everyone) and asked a direct question: can a shelter be forced unusable during a precycle
independent of the (region/room-limited) visual flood the previous round established?

**"Force Submerged Superstructure Precycles"** — straightforward: a new `Configurable<bool>`
(`PrecycleTimerSettings.ForceSubmergedSuperstructure`) that, when checked, skips the
`world.region.name == "MS" && !isRivulet` suppression already in the flood-roll code, so the flood
chance applies on MS too, for any character.

**"Force shelter unavailable"** — answered the question by finding vanilla ALREADY has almost
exactly this mechanic, just scoped far narrower than "any shelter, any region." Traced to
`ShelterDoor`'s own constructor (root namespace): when the room being constructed is the player's
current den and `rainCycle.preTimer > 0`, vanilla checks whether that SPECIFIC room's own
`roomSettings.placedObjects` contains a `PlacedObject.Type.BrokenShelterWaterLevel` marker -- a
level-design object only some regions' shelters were built with -- and only then sets
`World.brokenShelters[shelterIndex] = true` (plus `brokenShelterIndexDueToPrecycle`). Confirmed by
grepping every other place `brokenShelters` is read (`Room.cs`'s danger-type gating,
`HUD/FoodMeter.cs`, `Player.cs`, `Menu/FastTravelScreen.cs`, `Watcher/Sandstorm.cs`,
`ShelterDoor.Broken` itself) that NONE of them additionally check for visible water -- the flag
alone, not the flood, is what actually makes a shelter non-functional. So: **yes, answered directly
-- any shelter can be forced unavailable, independent of visuals**, by setting that same flag
without needing the room to ever have had the placed-object marker. Implemented as
`PrecycleForceShelterUnavailableHook`, a SEPARATE new Harmony Prefix on `ShelterDoor`'s own
constructor (not folded into `PrecycleTimerGimmick`'s existing `RainCycle`-constructor Postfix,
since this needs a completely different hook point) that sets `brokenShelters`/
`brokenShelterIndexDueToPrecycle` directly, gated on this gimmick being assigned, a new
`ForceShelterUnavailable` checkbox being checked, and vanilla's own same two preconditions
(current den, active precycle). Used a Prefix rather than Postfix specifically so vanilla's own
later `if (Broken) { ... }` block in the same constructor (which randomizes the broken door's visual
segment/flap offsets) picks up the forced value too -- the door's own broken-door visual should play
correctly; only the room-wide water level remains the separate, DangerType-gated system from the
previous round.

Five settings rows now on `PrecycleTimerGimmick`'s own screen (frequency; duration + force-max;
flood; force-submerged; force-shelter-unavailable) -- `GimmickRegistry.cs`'s own `PrecycleTimer`
entry picked up `settingsContentHeight: 260f` to give the scroll box room, matching Chains' own
precedent for a settings view that outgrew the plain viewport. **Not yet built or tested** -- this
round is a second, genuinely novel kind of intervention for this mod (forcing a vanilla mechanic
that's normally level-design-gated, via a brand new hook point on a constructor nothing else in this
mod has touched before), so test deliberately once a build exists: confirm a shelter with NO
BrokenShelterWaterLevel marker still becomes unusable when this checkbox is on, and confirm a
shelter's normal (non-precycle) behavior is completely unaffected when this gimmick isn't assigned
or the checkbox is off.

## 2026-09-18 (later still): character-design status audit, per-character No Hold defaults, Requirement settings architecture, Reach Ending requirement

Follow-up, same session. User supplied a target character/gimmick/requirement design table (a
screenshot of a spreadsheet) and asked for a status audit before any new code -- **saved as
`src/Primal/CHARACTER_DESIGN_STATUS.md`**, its own file, not duplicated here; read that doc for the
full row-by-row findings. Short version: Monk/Artificer/Gourmand/Spearmaster's gimmick rows were
already done (including a "double check" on Chains' visual chain placement that turned out to
already be correct -- `IsPlayerPipe`, used for the visual anchors, already excludes `CreatureHole`
while `IsSealableExit`, used for the actual door-sealing, correctly still includes it). Two real
architectural gaps were identified and both are now addressed by this same follow-up (see below);
a few items (Rivulet's "50% multiplier" Precycle Timer duration, "visit every shelter," Saint's
delayed-activation No Hold) still need the user's own design decision and weren't touched.

**Per-character default *values* for No Hold, resolved as a clarification, not a rebuild.** The
user pushed back that this was already supposed to work -- correct: re-reading `NoHoldGimmick.cs`
confirmed every category's `Configurable<bool>` was ALREADY bound under a
`characterID + "_NoHold_" + category.Key` key, i.e. Saint and Inv always had their own
independently-stored checkboxes. The actual (narrower) gap was that the DEFAULT fed into each
character's first bind came from one global `CategoryDef.DefaultBlocked` array, not a per-character
lookup. Fixed with `NoHoldSettings.PerCharacterDefaultBlocked` (`Dictionary<string,
HashSet<string>>`), the same `characterID == X ? ... : ...` pattern `RainTimerSettings`/
`BlizzardTimerSettings` already used for Saint's own numeric defaults, generalized to a set of
category keys: Spearmaster -> {Spears} (unchanged in practice), Inv -> {Spears, Yeeks, Squidcadas},
Saint -> all ten categories plus "Other" (everything). Saint's entry is NOT yet wired into
`GimmickDefaults.cs` as an actual default assignment -- the user's own design also wants No Hold to
only activate on Saint after one full cycle, a delayed-activation mechanic that doesn't exist
anywhere in this codebase yet (flagged, not built, per the audit doc's own open question).

**Requirement settings/config-menu architecture, added from scratch.** `Requirement` (base class,
`Requirement.cs`) gained `BindSettings`/`BuildSettingsUI`/`SettingsContentHeight`/`HasSettings` --
virtual properties defaulting to null/0/false, mirroring `GimmickDef`'s own fields exactly (same
names, same contract, same "build once per character into a cache" requirement since a
`Configurable<T>` can only ever be bound to one UI element at a time). `PrimalOptions.cs` wired the
consumer side to match: a new `DetailView.RequirementSettings` case, `_requirementID` field,
`OpenRequirementSettings`/`RenderRequirementSettingsModal` (the latter a close mirror of
`RenderGimmickSettingsModal`), and `EnsureConfigBound`'s own per-character Requirements loop now
calls `requirement.BindSettings?.Invoke(this, character.ID)` alongside the existing plain
enabled/disabled bind, exactly like the Gimmicks loop already does. The requirement row's own name
button in the Split view -- previously NEVER wired to an OnClick at all, by deliberate original
design, since no requirement had settings to show -- now gets `OpenRequirementSettings` wired up
*only* when `requirement.HasSettings` is true, preserving the original "click does nothing, not
greyed" behavior for the ~54 requirements that still have none.

**New requirement: Reach Ending**, the first real consumer of the mechanism above, resolving the
"story ending"/"Ascend" gap that appeared six times across the target table (Gourmand, Spearmaster,
Rivulet, Saint for "story ending"; Monk, Hunter for "Ascend") with no existing untimed requirement
to point at -- the only prior ascension-tracking requirement, `AscensionRequirement`, is a
38-minute SPEEDRUN challenge, a different thing not reused here. `ReachEndingRequirement`
(`Requirements.cs`, general, no `HomeCharacter`) reuses the same `standardVoidSea` flag off
`SurvivorEvents.OnGameEnding` every other ending-aware requirement in this file already reads, with
no time limit. Its own settings sub-view (`ReachEndingSettings`) is one per-character
`Configurable<string>` ("Story" or "Ascension", default "Story"), rendered via `OpListBox` -- the
same already-shipped, already-working control `ChainsSettings`' own quick-preset picker uses,
reused here for its second-ever consumer in this codebase rather than risking an untested new
control type. A character wanting the "Ascend" reading (Monk, Hunter) needs this switched to
"Ascension" once assigned -- not automatic from the ID alone.

No default assignments were added for any of these new pieces (Reach Ending itself, or No Hold onto
Saint) -- per the audit doc, several rows still need the user's own decision first. **Not yet built
or tested as of this doc** (no `.NET` toolchain in this container). When retesting: (1) confirm a
requirement WITHOUT settings (e.g. any of the 50 curated Community Challenges) still behaves
exactly as before -- unclickable name button, no visual change; (2) confirm Reach Ending's own name
button opens a settings modal showing the Story/Ascension list box; (3) confirm switching that list
box and reaching the corresponding ending actually reports completion, and reaching the OTHER
ending does not; (4) confirm No Hold's own per-character defaults now differ correctly for
Spearmaster/Inv/Saint the first time each is ever assigned (a fresh save, or Remix's own "reset to
defaults").

## 2026-09-18 (later still): every remaining CHARACTER_DESIGN_STATUS.md gap closed -- Precycle Timer percentage, No Hold inert-until-cycle-N, Ending renamed with Either + per-character defaults, Outlaw/FoodQuest/King renamed and configured, Visit Shelter requirement built

Direct follow-up to the audit round just above, same session: the user answered every "still needs
your own decision" item from `CHARACTER_DESIGN_STATUS.md` in one message and asked for all of it
built. That doc's own tables are now updated to match (read it for the per-row status instead of
duplicating the whole table here) -- this entry covers the mechanism-level "how" for each piece.

**Precycle Timer duration, fixed minutes -> percentage multiplier.** `PrecycleTimerSettings`'
`DurationMinutes`/`MaxDurationMinutes` are gone, replaced by `DurationPercent` (0-400, default 100,
same range/default convention `RainTimerSettings` already uses). `PrecycleTimerGimmick.Postfix` now
draws a THIRD roll (`durationRoll = UnityEngine.Random.value`) inside the same single
reseed/restore `UnityEngine.Random` bracket that already produces `triggerRoll`/`floodRoll` --
confirmed via re-reading vanilla's own constructor that its own duration roll
(`Random.Range(4800f, 12000f)`) happens inside that identical reseeded window too, immediately
after the trigger roll, so this keeps the same seeding discipline vanilla itself uses. Both of
vanilla's own bounds (`VanillaMinDurationTicks = 4800`, `VanillaMaxDurationTicks = 12000`) are
scaled by the percent (`Mathf.RoundToInt(bound * (percent / 100f))`), then `durationTicks` is
rolled between the two SCALED bounds -- or, "Force max precycle timer" checked, always the scaled
upper bound with no roll, which is now a much closer analogy to Remix's own
`MMF.cfgNoRandomCycles` ("always pick `cycleTimeMax`") than the old fixed-999-minutes version was.
Resolves the audit's own "50% multiplier doesn't map to anything that exists" gap. No specific
percent/frequency was requested for Rivulet's own default, so Precycle Timer is not yet assigned to
Rivulet or tuned to any particular values -- fully available by hand via Remix.

**No Hold gains "inert until cycle N," a brand-new mechanic, specific to this one gimmick.**
`NoHoldSettings` gained a fourth per-character `Configurable<int>` (`_inertUntilCycle`,
`GetInertUntilCycle`, default 0, range 0-999) alongside the ten category booleans and `_blockOther`
-- a textbox in its own settings sub-view, below the existing 2x5 category grid. `NoHoldGimmick`'s
own `Postfix` (on `Player.Grabability`) now reads `GimmickGate.GameFor(player)
.GetStorySession.saveState.cycleNumber` (the same 0-indexed source `Requirements.cs`'s own
"starting cycle" checks already use) and returns early -- doing nothing at all, every category
still unblocked -- whenever that number is below the configured threshold; once it's reached, every
category behaves exactly as before. 0 (the default) means "no delay," so this is fully
backward-compatible for every character that doesn't touch the new setting. Saint's own value was
NOT set to a specific number -- the user introduced this as a new general concept, not as a
specific requested starting value for Saint, so guessing one would have been assuming scope that
wasn't there; it stays at the generic default (0) and is fully settable per-character via Remix.
Saint's own "everything blocked" category defaults (`PerCharacterDefaultBlocked`) were already
built in the prior round and are now actually wired into `GimmickDefaults.cs` as a real default
assignment for the first time.

**"Reach Ending" renamed to "Ending," gains a third "Either" option, gains real per-character
default select values, and gains real default assignments -- resolving the "Ascend"/"story ending"
gap for all six rows in the design table, not just building the mechanism.** `DisplayName` is now
"Ending" (ID stays `"ReachEnding"` -- no config keys, save markers, or curated-ID lists exist yet on
any real save file, so nothing needed to churn just because the display name did).
`ReachEndingSettings.Satisfies(characterID, standardVoidSea)` replaces the old bare
`WantsAscension` boolean check: `"Either"` always returns true regardless of which ending type just
happened; `"Story"`/`"Ascension"` behave exactly as before. The select's own DEFAULT value is now
per-character (`PerCharacterDefaultEnding`, a `Dictionary<string, string>` -- the same
"characterID == X ? Special : Generic" idea generalized to a lookup table since there are two
special cases here, not one): Monk and Hunter default to `"Ascension"`; every other character falls
back to `"Story"`. `RequirementDefaults.cs` (new file, see below) defaults this requirement onto
Monk, Hunter, Gourmand, Spearmaster, Rivulet, and Saint -- the exact six rows the original audit
identified as sharing this same gap.

**A real default-assignment mechanism for Requirements finally exists: `RequirementDefaults.cs`.**
The Requirements-side counterpart to `GimmickDefaults.cs`, same contract (only ever takes effect on
the reserved slot, fully overridable per character via Remix) and same reasoning
(`GimmickAssignments.SetDefault` is genuinely generic over gimmick and requirement IDs, confirmed
back when `Requirement`/`RequirementRegistry` were first built). Wired into
`RainWorldHooks.Postfix` right after `GimmickDefaults.Apply()`, same ordering requirement (must run
before `EnsureConfigBound()`, which reads these as each `Configurable`'s bound default). Covers:
Ending -> Monk/Hunter/Gourmand/Spearmaster/Rivulet/Saint (above), Outlaw -> Hunter, GourmandFoodQuest
-> Gourmand, InvHighestPoint (King) -> Inv (Sofanthiel), VisitShelter -> Rivulet.
**Deliberately not included** (flagged in the file's own header comment rather than silently
assumed): `SpearmasterFastEndingRequirement` -> Spearmaster and `UniqueKillsRequirement` ->
Artificer both appear in the original design table too, but neither was explicitly re-requested in
the message that asked for everything else here.

**Outlaw's score threshold, hardcoded -> configurable, and raised to the requested 100k.**
`OutlawRequirement`'s old `private const int RequiredScore = 10000` is gone; a new
`OutlawSettings` class (mirroring every other per-character-settings class in this file) gives it a
per-character `Configurable<int>` textbox (`OutlawSettings.GetRequiredScore`), defaulting to
**100,000** -- a real behavior change from the old 10,000 for anyone who doesn't touch the new
setting, per the explicit "100k points" request. `OutlawScoreScreenHook`'s own detection now reads
`OutlawSettings.GetRequiredScore(characterID)` instead of the old constant. Defaulted onto Hunter.

**"Food Quest & Save" renamed to "Food Quest."** `GourmandFoodQuestRequirement.DisplayName` is a
one-line change; `ID` ("GourmandFoodQuest") is untouched. Defaulted onto Gourmand (alongside
Ending/Story from the section above).

**"Highest Point" renamed to "King," and its four hardcoded conditions became four independent,
per-character checkboxes.** `InvHighestPointRequirement.DisplayName` is now "King" (`ID`/
`RequirementID`/`TargetRoom` all unchanged). The four conditions the old `CheckPlayer` hardcoded as
simultaneously required (standing in the room's own highest screen, wearing the cloak, holding a
slugpup, carrying a vulture mask) are now four separate `Configurable<bool>` toggles in a new
`InvHighestPointSettings` class, each gating its own `if` inside `CheckPlayer` rather than one
combined `&&` chain -- all four default to checked, exactly reproducing the old all-required
behavior until a player unchecks one. `SettingsContentHeight` overridden to 200f (four checkbox
rows). Defaulted onto Inv (Sofanthiel).

**New requirement: Visit Shelter, researched and built rather than left unimplemented.** The
original audit had flagged this as needing new tracking code with an open scope question (one
region, or the whole game). Found a genuinely buildable path using three pieces of REAL, existing
vanilla tracking (confirmed via decompilation, nothing here reimplements "has this room been
visited"):
  - `World.shelters` (`int[]`, populated by `WorldLoader` from every room tagged SHELTER or
    ANCIENTSHELTER in that region's own world file) -- a region's complete shelter roster, once
    that region is loaded.
  - `RegionState.roomsVisited` (`List<string>`, persisted on the save file, grown by
    `RegionState.ReportRoomVisited` every time `Player.NewRoom`/`Player.Update` reports a genuinely
    new room -- the exact same list the game's own map-discovery texture reads) -- a region's own
    "have I been here" record, reachable off any live `World` as `World.regionState` (a null-safe
    computed property vanilla itself already defines).
  - `SlugcatStats.SlugcatStoryRegions(SlugcatStats.Name)` -- the character's own required campaign
    region list, used to decide when EVERY region is done, not just one.

`VisitShelterRequirement` (`Requirements.cs`, `HomeCharacter` = Rivulet) subscribes to
`SurvivorEvents.OnPlayerEnteredRoom` (the standard "just arrived" hook, cheaper than a per-tick
patch since the answer can only change on arrival somewhere new). On each firing: if every shelter
in the CURRENTLY LOADED region (minus anything `World.DisabledMapRooms` removes for this
character's own timeline -- the same exclusion vanilla's own pup-spawn placement logic already
applies to this same list) is now in that region's own `roomsVisited`, this character's own
"region complete" marker for that region is persisted via `PrimalSaveMarkers`
(`Marker("PrimalShelterRegionComplete", characterID, world.name)`) -- so this is only ever
recomputed once per region per save, not every time the player passes back through an
already-completed one. Completion is reported the moment EVERY one of
`SlugcatStats.SlugcatStoryRegions(characterID)` has that marker. **Scope resolved as "every shelter
in every story region"** (the whole game), the more literal reading of "every shelter has been
visited" with no region qualifier in the request -- flagged in the class's own doc comment as a
small, contained change to a per-region-only scope if that turns out to be what was actually
wanted (drop the all-regions check, report as soon as one region's own shelters are done).
Optional/side regions (`SlugcatOptionalRegions`) are deliberately excluded from the requirement,
since not every run reaches them and requiring one could make this permanently uncompletable
through no fault of thoroughness. Defaulted onto Rivulet.

**Follow-up needed later: which shelters actually get required has NOT been playtested and needs
future review.** `AllSheltersVisitedInLoadedRegion` (`VisitShelterRequirement`, `Requirements.cs`)
takes `World.shelters` as ground truth minus only `World.DisabledMapRooms` -- both ANCIENTSHELTER
and SU_S05 (a shelter the vanilla pup-spawn code specifically excludes for its OWN unrelated
reasons, see that code's own comment) are currently left IN the required set, on the reasoning that
a real player probably still can walk into either. That reasoning is untested. Revisit once a real
playthrough is available: if either turns out to be unreachable, decorative-only, or otherwise not
meant to be "visited" in the way this requirement intends, exclude it explicitly the same way
`DisabledMapRooms` already is. Also worth re-checking then whether `world.shelters` ever includes a
shelter that's real but permanently inaccessible for a structural reason this doc comment's own
research didn't surface (region-specific dev/cut content, a shelter behind a permanently-locked
gate for this character's own timeline, etc.).

**Not yet built or tested as of this doc** (no `.NET` toolchain in this container, same as every
other round). When retesting: (1) Precycle Timer's percent scaling at a few different values
(0%, 100%, 400%, and with "Force max" checked) against the logged duration-vs-bounds line; (2) No
Hold's inert-until-cycle-N on a character with a nonzero value set by hand, confirming grabs are
allowed before the threshold cycle and blocked from it onward; (3) Ending's three-option list box
(including the new "Either" entry) and that Monk/Hunter really do start on "Ascension" on a fresh
save while everyone else starts on "Story"; (4) Outlaw's textbox actually gates completion at
whatever value it's set to, not just the old hardcoded 10000; (5) King's four checkboxes
independently gating completion (uncheck one, confirm the other three alone are no longer
sufficient); (6) Visit Shelter actually accumulating region-complete markers correctly across a
multi-region playthrough, and firing only once the LAST region's LAST shelter is reached -- the
one piece here with no small-scale way to verify short of a long real playthrough.

## Outstanding items

### Status at a glance (2026-09-15)

Everything below the table is the long-form history for each item; the table is the "what do I test
next" summary. **Nothing since the Chains core mechanic has been confirmed in-game** — roughly 16
commits of unbuilt, untested work, by explicit user decision ("we'll just have to test everything
at the same time, bit by bit, to bring it back probably").

| Area | State | First thing to check |
| --- | --- | --- |
| **Whole-codebase refactor (2026-09-15)** | Unbuilt | Does it compile at all? It touches ~20 files. Then: does any gimmick still fire? |
| SlugBase dependency removed | Unbuilt | Mod loads with SlugBase *uninstalled* |
| Primal character names + "not implemented" labels | Unbuilt | All 10 names on the character-select screen |
| Blacked-out portraits (6 characters) | Unbuilt | Survivor/Monk/Hunter vs Rivulet/Saint — different shaders, may diverge |
| Slugpup rewrite (isSlugpup, not SlugCatClass) | Unbuilt | Gourmand looks fat *and* pup-sized |
| Slugpup "ends on beating the game" | Unbuilt | Also fires for a non-Gourmand character's own alt ending |
| Kill List gimmick | Unbuilt | Kill → shelter → button on cycle-end screen |
| Threat Slowdown / Threat Panic | Unbuilt | Monk, with threat rising |
| Chains gimmick | **Core confirmed in-game**; settings import unbuilt | The ported banlist/preset settings |
| Primal Slot tab-switch | **Confirmed in-game** through round 3 | Rounds 4–5 (character preservation, multiple Primal slots) |
| Config menu (grid + detail panel) | **Confirmed in-game** 2026-09-13 | The new slot-switch key binder in the title row |

**Suggested testing order for the refactor specifically**, since it's the change most likely to
break everything at once and the hardest to bisect: (1) does it build; (2) does the mod load with
SlugBase uninstalled; (3) does the Remix screen still open and still show/save assignments (that
exercises `GimmickAssignments`' new key struct, `PrimalOptions`' dropped dictionary, and the new key
binder); (4) does any one simple gimmick still fire — Misery on Gourmand is the cheapest check, and
it exercises `GimmickGate` end to end; (5) then the save-marker features (OneSpear's pull count,
Kill List progress, Slugpup's ending flag), which all now share `PrimalSaveMarkers`; (6) then the
per-frame ones (Chains, both threat gimmicks) for behavior *and* frame rate.

*(Status as of this doc's last update: the user confirmed BOTH previously-outstanding drill-down
bugs are resolved — the character grid overflow fix (5x2 layout) and the Gotcha #4
`MultiuseConfigurableException` fix both re-tested and working. The user then asked for the
character-grid/detail-panel UI merge (see "Config menu UI, in detail"); reported as **not working
as designed**. Next feature — Primal character names for Gourmand/Spearmaster on the alt slot (see
"Primal character names, in detail") — reported as **doing nothing at all**. Investigating the
second report found a likely explanation for both (see **Gotcha #5**, new): the locally-built DLL
in the connected source tree predates *every* source change made that day. The user then ran
`dotnet build` directly and it **failed outright** — `Gimmicks/AltCharacterIdentity.cs`, leftover
dead code from a discarded design, still called a method (`PrimalSlot.SetCurrentAltCharacterID`) that
no longer exists. The project has likely not compiled successfully since whichever session removed
that method, so an unknown number of past "confirmed working" test results may have actually been
against an old DLL too — treat anything not re-verified after this fix with a little extra
suspicion. Fixed by excluding that one file from the build (`Primal.csproj`); every other
file, including both of today's untested features, compiled clean once it was out of the way.
The user re-ran the build clean and, on retesting, reported the in-game character select screen
loading slowly into an almost-blank/wireframe-only state — not a direct answer to either pending
check-item. Root-caused to a **second, independent** piece of leftover dead code from the same
discarded design #2 (see **Gotcha #6**, new): `CharacterSelect\SlugcatSelectMenuHooks.cs`, still
compiled and still an active Harmony patch on `SlugcatSelectMenu.Update`, is broken by construction
on the alt slot specifically (its "which roster is visible" check can never return true for any
character once nothing populates `AltRoster`'s registry anymore), so it spins its full page-search
loop every frame with no valid target. Fixed the same way as Gotcha #5 — excluded `Characters\**`
and `CharacterSelect\**` from compilation via `Primal.csproj`. The user rebuilt and reported
**"its the same thing"** — checking real evidence (retried `device_request_folder_access` for the
Steam game root, granted this time) turned up the actual `exceptionLog.txt`, confirming the precise
mechanism: `SlugcatSelectMenuHooks.Postfix` calling `Menu.SlugcatSelectMenu.UpdateSelectedSlugcatInMiscProg()`
directly throws `MethodAccessException` every frame (a Gotcha #1-class bug on a method instead of a
field). Re-checking the `.csproj` **on the device** (not just this session's own copy) found why the
rebuild changed nothing: the earlier `device_commit_files` push had reported success but never
actually landed — the file on the user's machine was still the old, pre-Gotcha-#6 content. Re-pushed
it and this time verified the write by reading the file straight back from the device before saying
anything further. The user rebuilt and copied the file again and reported **the same issue,
nothing changed** a second time. Checked real evidence again rather than re-theorizing:
`bin/Debug/Primal.dll` had genuinely shrunk to 31,744 bytes this time (proof the fix compiled
correctly), but the **installed** copy at `...\StreamingAssets\mods\Primal\plugins\
Primal.dll` was still the old 34,304-byte file — the manual copy step into the actual game
folder hadn't happened, so the game was still loading the broken DLL (confirmed: fresh
`exceptionLog.txt` showed the byte-for-byte identical `MethodAccessException`). Since this session
already had folder access to the Steam game root, copied the fixed DLL there directly with
`device_commit_files` and verified the copy landed (31,744 bytes, fresh timestamp) rather than
asking the user to redo the manual step. **Both "nothing changed" reports turned out to be deploy
gaps (first this session's own doc push failing silently, then the user's manual game-folder copy
being missed), not logic bugs — the fix itself compiled clean both times it was actually tried.**
**Confirmed fixed (2026-09-11):** the user relaunched and confirmed the character select screen
works now. **Next requested step (user, same session): delete the leftover SlugBase alt-character
JSON files entirely** (not just excluded from compile — actually removed), since they're confirmed
unnecessary and are what SlugBase is really loading at runtime regardless of any C# exclusion (see
Gotcha #6's discovery that the `.csproj` exclusion only stops the *C#* registration/patch code, not
SlugBase's own independent JSON-file loading). Still outstanding after that: the two
originally-requested items (UI merge appending below the grid; Primal Gourmand/Spearmaster
names+descriptions) have still never been checked against a build with no known interference left
in it.)

- **Done (2026-09-11): all leftover discarded-design-#2 artifacts deleted, not just excluded.** The
  user deleted, by hand (this session has no delete capability — only list/stage/commit/request-
  access):
  - `C:\Program Files (x86)\Steam\steamapps\common\Rain World\RainWorld_Data\StreamingAssets\
    mods\Primal\slugbase\` (the copy SlugBase actually loaded into the running game).
  - `C:\Users\rokas\Documents\RWMods\src\Primal\mod\slugbase\` (the source tree's master copy
    of the same 9 JSON files).
  - `C:\Users\rokas\Documents\RWMods\src\Primal\Characters\` (9 old SlugBase character
    classes + `AllCharacters.cs`).
  - `C:\Users\rokas\Documents\RWMods\src\Primal\CharacterSelect\` (old roster-filtering UI
    hooks — `AltRoster.cs`, `SlugcatSelectMenuHooks.cs`).
  - `C:\Users\rokas\Documents\RWMods\src\Primal\Gimmicks\AltCharacterIdentity.cs`.
  All confirmed gone via `device_list_dir` re-checks. Since the folders/file these `<Compile
  Remove>` entries pointed at no longer exist, removed all three exclusion lines and their two long
  explanatory comments from `Primal.csproj` too (harmless to leave pointing at nothing, but
  no reason to) — verified the trimmed file is still well-formed XML, pushed it, and confirmed via
  device readback (2,966 bytes, down from 6,218). **Not yet re-verified that the project still
  builds clean after this csproj trim** — should be a pure no-op for the compiler (removing
  `<Compile Remove>` entries for files that no longer exist changes nothing about what compiles),
  but worth a `dotnet build` check next time rather than assuming.
  - If any of these reappear later, it means something is still generating them — that would itself
    be a bug worth investigating (per the user's own earlier framing).
- **New, needs build + in-game re-test (2026-09-11, fourth attempt — see the "struggle" framing at
  the top of "Slugpup gimmick stats rescaling, in detail" above for the full blow-by-blow): Slugpup
  gimmick stats + color.** Attempt 1 (stats-only, hooked `Player`'s constructor): built, deployed,
  tested — user reported "no change" (root-caused: hook fired one step too late, after BodyChunk
  mass was already baked in) — **and separately reported the pup renders in the wrong color**, a
  second, independent bug found at the same time (`ShortCutColor()` bypasses the real character's
  palette once `SlugCatClass == Slugpup`). Attempt 2/3 fixed both: `SlugpupStatsGimmick` moved to
  Postfix `NPCStats`'s constructor instead (fires before BodyChunks are built), and a new
  `SlugpupColorGimmick` Postfixes `Player.ShortCutColor()` to restore the real character's color
  via vanilla's own `PlayerGraphics.SlugcatColor(...)`. User ran `dotnet build` on this and got a
  real compile error (attempt 4 territory): `CS0246: The type or namespace name 'NPCStats' could
  not be found` — `NPCStats` turned out to be a type nested inside `Player`
  (`TypeAttributes.NestedPublic`, confirmed via the game assembly's own `NestedClass` metadata
  table), so the attribute needed `typeof(Player.NPCStats)`, not `typeof(NPCStats)`. Fixed and
  pushed, verified landed on the device (21,606 bytes). **Still not yet built (no toolchain in this
  container — see "Container environment notes") or tested in-game at all — every attempt so far
  has found a new, different way to be wrong (silently-too-late timing, a second unrelated bug, a
  compile error), so treat "should work now" with real skepticism until the user actually confirms
  it in-game.** When retesting: confirm the build actually succeeds first (nothing else has been
  checked since the `Player.NPCStats` fix), then confirm Primal Gourmand's body no longer looks
  thin as a pup AND renders in Gourmand's own tan/pink color, and check the BepInEx log for
  `[Slugpup] Gourmand: npcCharacterStats rescaled from real stats (bodyWeightFac X -> Y)` (X should
  be noticeably larger than Y) to confirm the stats patch actually ran.
- **New, not yet built or tested (2026-09-15, fifth Slugpup round — see "Slugpup gimmick:
  SlugCatClass-flip rewrite, in detail" above for the full account): the stats/color fixes above
  got tested in-game and confirmed the color was now correct, but Gourmand still "wasn't fat
  enough... looks like a slugpup" instead of a fatter version of himself. Root cause: the
  `SlugCatClass`-flip architecture itself, not a missing rescale — flipping `SlugCatClass` away
  from `Gourmand` meant `PlayerGraphics`' own Gourmand-specific 1.4×/1.6× fatness scale could never
  fire. Rewrote the whole gimmick to match how the reference "Pupify" mod actually does it (found by
  re-reading `reference/ExampleMods/Pupify/Pupify.decompiled.cs`, already committed in this repo, no
  live disassembly needed this round): force `Player.isSlugpup`'s getter to `true` directly, and
  never touch `SlugCatClass` at all. Also found and fixed a second, independent, previously-missed
  bug in the same pass: `Player.SetMalnourished` unconditionally rebuilds `npcCharacterStats` from
  scratch any time malnourishment changes, silently undoing the rescale — fixed with a new
  `SlugpupMalnourishmentStatsGimmick` Postfix. **When retesting, check all of, in order: (1) it
  compiles; (2) pup-ified Gourmand shows the same fat head/body proportions a normal adult Gourmand
  does, just smaller, not the plain narrow Slugpup shape; (3) still renders in Gourmand's own
  tan/pink for both main body color and shortcut/pipe-transition tint; (4) pup-scale physical stats
  are still correctly applied (check the `[Slugpup] ...` log line); (5) eat until malnourished (or
  recover from it) and confirm the pup-scale stats are STILL correctly rescaled afterward, not
  reverted to full-adult or generic-pup sizing — the new, specific regression case found this round;
  (6) any of Gourmand's own `SlugCatClass`-gated behaviors/abilities that used to break under the
  old flip now work correctly while pup-ified.**
- **New, not yet built or tested (2026-09-15, sixth Slugpup round, a feature not a bugfix, revised
  twice after two separate user corrections — see "Slugpup gimmick: ends at Outer Expanse, in
  detail" above for the full three-round account): beating the game via any "standard" (non-
  ascension) ending now permanently stops the Slugpup gimmick for that character on that save file
  (still shown assigned in Remix -- just inert), gated by a new `Configurable<bool>`
  (`SlugpupEndingSettings.EndsOnBeatingGame`) that **defaults to on**, per explicit request.
  **Round 1 hooked the wrong thing** (`RainWorldGame.BeatGameMode`'s `standardVoidSea: true`
  branch, actually the separate void-sea/ascension ending -- caught by the user: "that is why its
  called standardVoidSea. look for a different ending"). **Round 2 hooked a Postfix on
  `MoreSlugcats.MSCRoomSpecificScript.OE_GourmandEnding.Update`** (a misleadingly-named but
  genuinely generic "reached Outer Expanse's western edge" handler), which matched but didn't
  generalize -- the user asked directly: "if it gets placed on some other character, say
  artificer, and they beat the game with standardVoidSea: false then that should also remove the
  slugpup effect," and it didn't, since that class only ever fires for the Outer Expanse
  walk-ending, never Rivulet/Artificer/Saint/Spearmaster's own alt endings elsewhere. **Round 3
  (the fix): back to a Postfix on `RainWorldGame.BeatGameMode` after all, but checking
  `!standardVoidSea`** (any standard ending) instead of `standardVoidSea` (round 1's mistake) --
  this correctly covers every character's own alt ending plus the Outer Expanse walk-ending in one
  hook, while still leaving the ascension ending untouched. Simpler than round 2 too: no
  `ConditionalWeakTable` needed, since `BeatGameMode` only ever fires once per ending event. New
  `SlugpupEndingMarker` (a permanent per-character `unrecognizedSaveStrings` marker, same mechanism
  `OneSpearGimmick` already uses). Needed no changes to `SlugpupStatsGimmick`,
  `SlugpupMalnourishmentStatsGimmick`, or `SlugpupColorGimmick` -- they all key off
  `Player.isSlugpup` (directly or via vanilla's own `NPCStats`-construction gate), so the one new
  check in `SlugpupGimmick`'s own Postfix is enough to turn the whole gimmick off from a single
  choke point. **When testing: (1) it compiles; (2) the new checkbox shows up in Slugpup's settings
  sub-view, checked by default, labeled around "beats the game"; (3) walking to Outer Expanse's
  western edge (not the void sea route) with it checked logs `[Slugpup] ... beat the game` and the
  character plays as their normal adult self from then on, for the rest of that save; (4)
  **assign Slugpup to a different character (e.g. Artificer), have them reach their own alt ending
  (LC_FINAL), and confirm the same effect -- the specific generalization the user asked to
  verify**; (5) unchecking the setting for a character before they beat the game means the gimmick
  keeps working after they do; (6) reaching the void-sea/ascension ending specifically does *not*
  trigger this (confirming the true/false distinction from round 1 stuck); (7) beating the game
  twice on an already-ended character doesn't break anything.**
- **New, built, deploy-gap already found and fixed, not yet tested in-game (2026-09-12): the Chains
  gimmick.** Requested by the user as "find the artificer chains mod and turn that into a gimmick"
  — turned out to be a **local, not-on-the-Workshop** mod already installed on the user's machine
  (not findable by web search or by cross-referencing their subscribed Workshop items; the user
  clarified it directly once asked). Reverse-engineered and ported into `Gimmicks/ChainsGimmick.cs`,
  registered in `GimmickRegistry`, and default-assigned to Artificer in `GimmickDefaults` — see
  "Chains gimmick, in detail" above for the full trace, what was and wasn't ported, and why.
  The user ran `dotnet build` and it **succeeded** (first build attempt for this file, first time
  around, unlike Slugpup's `CS0246`) — but then reported "config says no gimmick applied to
  artificer, nor does a new [gimmick] exist [in the Remix menu at all]". This was **not** a logic bug
  in `ChainsGimmick.cs` or `GimmickRegistry`/`GimmickDefaults` — it was the exact same deploy gap as
  **Gotcha #5**, recurring: `dotnet build` only updates the source tree's own `bin/Debug/` (and this
  project's post-build step mirrors that into `mod/plugins/Primal.dll`, confirmed in sync,
  42,496 bytes) — it does **not** touch the actually-installed copy the game loads,
  `...\StreamingAssets\mods\Primal\plugins\Primal.dll`, which was still the old,
  pre-Chains 33,792-byte build. Confirmed via `device_list_dir` on both paths before touching
  anything (the installed copy's own timestamp predated `ChainsGimmick.cs` entirely) rather than
  guessing. Since this session already has folder access to the Steam game root (from Gotcha #5's
  original fix), copied the fresh DLL+PDB there directly — first `device_commit_files` attempt
  reported success but the size hadn't actually changed on re-check (the same recurring silent-write
  glitch documented throughout this file's Slugpup section), retried with `force: true`, and this
  time verified the installed copy is genuinely 42,496 bytes, matching the fresh build exactly.
  Also spot-checked (2026-09-12) that `PrimalOptions.cs` builds every one of its
  character/gimmick UI loops from `GimmickRegistry.All` dynamically (four separate `foreach`
  sites, grepped directly) rather than any hardcoded gimmick list, so no second file needed editing
  for Chains to actually appear once the right DLL is loaded. **Still never confirmed working
  in-game** — next: relaunch, open Remix on the alt slot, confirm "Chains" now exists as an
  assignable gimmick and shows enabled by default on Artificer, then confirm the actual mechanic
  (entering a creature-populated room seals the exits and shows chains, killing everything reopens
  them, and — if convenient — the escape timer's fallback unlock fires if a room is left unfinished
  long enough). This gimmick is still a first-ever in-game test with zero prior rounds and no
  build/logic issues found yet, which is different from "confirmed correct" — treat it with the same
  skepticism as every other freshly-written gimmick in this doc, arguably more so given the
  soft-lock risk if the creature/chain bookkeeping turns out to have a bug once actually run.
  **Lesson reinforced, not new:** after ANY successful `dotnet build`, the fresh DLL still has to be
  manually copied into the Steam game's installed mods folder before it takes effect in-game — a
  clean build alone proves nothing about what the running game is actually loading.
  **Update (2026-09-12): first in-game round happened.** The user confirmed the core mechanic now
  works — entering a room with creatures in it does lock. They also found a real gap: a creature
  that wanders into a room *after* the player enters it (when that room had zero qualifying
  creatures at entry, so no lock ever triggered) wasn't detected or added at all. Asked for the
  source mod's own already-debugged fix to be reused rather than reinvented — re-disassembled and
  ported `Artificer_Hell.Creature_NewRoom` + `CustomHRGuardManager.AddNewCreature` faithfully (new
  `ChainsCreatureArrivalHook` Harmony patch on `Creature.NewRoom`, plus `HandleCreatureArrival`/
  `TryAddArrivingCreature`/`FlushArrivalBuffer` in `ChainsGimmick.cs`) — see "Chains gimmick, in
  detail" above for the full trace, including the one source-mod bug (a missing `GetCreatureList`
  `.Clear()`) deliberately fixed rather than reproduced. Built clean, copied to the installed game
  folder, and verified byte-for-byte (38,724 bytes) — same silent-write glitch hit on the first
  `device_commit_files` attempt (reported success, size unchanged on re-check), fixed the same way,
  with `force: true` and a re-verify. The user then ran `dotnet build` again for this change and it
  **succeeded** (`bin/Debug/Primal.dll` 44,544 bytes, `mod/plugins/` mirror confirmed in
  sync) — same deploy-gap lesson applied proactively this time: copied the fresh DLL+PDB straight to
  the installed game folder myself before reporting anything done
  (`...\StreamingAssets\mods\Primal\plugins\`, previously the pre-arrival-handling
  42,496-byte build). Same silent-write glitch hit again on the first `device_commit_files` attempt
  (mtime updated, size still 42,496/15,184 on re-check), fixed the same way with `force: true`,
  re-verified at 44,544/16,084 bytes — genuinely matching the fresh build this time. The user then
  confirmed in-game: "it looks like its implemented properly now" — both the original lock/unlock
  mechanic and the new-arrival fix are working.
  **Update (2026-09-12): settings import.** The user then asked to import the rest of the source
  mod's own Remix settings screen, specifically calling out its creature-allowlist selection.
  Disassembled `Artificer_HellOptions` (not previously examined in full) and ported its entire
  option surface: a 12-checkbox creature-type banlist (`ChainsSettings.BanOptions`, replacing the
  old fixed `BannedTypes` set and now shared identically by both the initial room scan and the
  arrival path, which previously used two different exclusion rules), a "catch late arrivals"
  toggle (`CatchLateArrivals`, gating the arrival-handling feature added earlier this session,
  defaulted on unlike the source's own off-by-default `genocideMode`), and a new global
  "faux-hunted burden" effect (`FauxHuntedBurden` + a new `ChainsHuntedModeHook` patch on
  `ArtificialIntelligence.Update`, off by default) — see "Chains gimmick, in detail" above for the
  full trace, including the exact preset contents recovered from `CustomHRGuardManager`'s own
  constructor and why "squids" turned out to mean cicadas. Pushed the updated source
  (`ChainsGimmick.cs` grew from 38,724 to 54,773 bytes, verified byte-for-byte -- same silent-write
  glitch hit again on the first `device_commit_files` attempt, fixed the same way with
  `force: true`) — this only updates the source tree, not any DLL, so it alone doesn't require a
  redeploy; a build is what's needed next.
  **First `dotnet build` for this change failed**: `CS0029`/`CS0019` in the new
  `ChainsHuntedModeHook` -- `AbstractWorldEntity.Room` returns an `AbstractRoom`, not the realized
  `Room` needed for a same-room comparison against another creature (everywhere else in this file
  already correctly used a realized `Creature`'s own `.room` field instead; this new method was the
  one place that reached for the wrong one). Fixed by rerouting through
  `owner.realizedCreature?.room` / `player.room` instead of the `.Room` property; pushed the fix
  (`ChainsGimmick.cs` now 55,143 bytes, verified). The user's next `dotnet build` **succeeded**
  (`bin/Debug/Primal.dll` 48,640 bytes) and they copied it to the installed game folder
  themselves.
  **Update (2026-09-12): quick-preset select menu + Artificer rename.** The user asked for the
  banlist select menu back (it had been replaced by checkboxes in the settings-import pass) and to
  rename Artificer to "Primal Artificer." Disassembled the real `Menu.Remix.MixedUI.OpListBox` from
  `lib/PUBLIC-Assembly-CSharp.dll` to get its actual constructor from metadata rather than guessing
  (see "Chains gimmick, in detail" above for the exact signature and `Configurable<string>.OnChange`
  usage) — added a `_banlistPreset` quick-preset control to `ChainsGimmick.cs` that sets the 10
  non-grouped type checkboxes at once, on top of (not instead of) the existing checkboxes.
  Separately, added Artificer to `PrimalCharacterNameHook`'s `if`/`else if` chain in
  `PrimalCharacterNames.cs` → "PRIMAL ARTIFICER" (see "Primal character names, in detail" above).
  Pushed both files (`ChainsGimmick.cs` 60,243 bytes, `PrimalCharacterNames.cs` 7,612 bytes,
  verified).
  **First build for this change failed**: `CS1503` x2 on the new `OpListBox` call, caused by the
  same class of mistake as Gotcha's own build-error entries above but in signature *decoding* this
  time rather than API confusion -- byte `0x07` was misread as `int` instead of `ushort` in the
  original hand-decode of `OpListBox`'s constructor, which knocked the intended overload out of
  resolution and made the compiler point at the wrong parameter entirely. Fixed by casting to
  `(ushort)` at the call site and re-verifying the signature with a proper decoder script written
  for exactly this purpose (`scratchpad/dnparse/sigdecode.py`) instead of by eye. Also noticed
  while re-staging before this push: the user has hand-edited Artificer's description line in
  `PrimalCharacterNames.cs` directly on their machine (now a vanilla-flavored line instead of this
  session's generic one) -- left untouched, not overwritten. Pushed the fix
  (`ChainsGimmick.cs` now 61,173 bytes, verified). **Not yet rebuilt or confirmed in-game** — next:
  `dotnet build` again, and if clean, (1) open Chains' settings for a character and confirm all
  controls including the new select menu render without clipping or throwing inside the
  drill-down's ~600×600 space, and that both the select menu and the individual checkboxes actually
  change what triggers a lock; (2) on the alt
  slot, confirm Artificer now shows "PRIMAL ARTIFICER" with a description listing Chains, the same
  way Gourmand/Spearmaster already should (see the very next bullet).
  **Update (2026-09-13): pipe-type shortcuts weren't sealed — then a first fix, then a user
  correction.** The user reported it directly: a locked room's "internal pipes marked by triangles"
  stayed open and let both the player and creatures walk straight through them out of an
  otherwise-sealed room, while only ordinary doors were actually being chained. Re-disassembled the
  source mod's own `LockPlayer` to check whether this was a porting gap — it wasn't; the source mod
  has the exact same gap (`shortCutType == Type.RoomExit` only, byte-for-byte identical to what this
  port already had), so this was a real gameplay bug in both, not something lost in translation.
  Dumped `ShortcutData.Type`'s full member list straight from the game assembly's own metadata
  (`Normal, RoomExit, CreatureHole, NPCTransportation, RegionTransportation, DeadEnd`) rather than
  guessing, and confirmed `Room.ShorcutEntranceHoleDirection` (used by both `UpdateLock`'s shove-back
  and `MultiChain`'s anchor placement) is purely geometric with no `Type` check in its own
  disassembly, so it's safe to seal more types without risking either call site.

  **First fix (sealed the wrong pair, later corrected):** guessing from names alone, sealed
  `RoomExit`/`NPCTransportation`/`RegionTransportation` and left `CreatureHole` unsealed. Pushed and
  verified (`ChainsGimmick.cs`, 66,527 bytes) but never confirmed in-game before the user caught the
  mistake by inspection.

  **User correction, authoritative:** "room to room PIPES - locked / within room SHORTCUTS -
  unlocked / creature DENS - locked / within room TRANSPORT for creatures - unlocked." Re-grepped
  the source mod's *entire* disassembly (not just `LockPlayer`) for every `ShortcutData.Type`
  reference anywhere in it: confirmed the source mod's own compiled code never implements any of
  this fine-grained per-type distinction at all (only ever `RoomExit`/`DeadEnd`) — so this
  categorization is the user's own domain-knowledge correction, not something recovered from the
  reference mod. `NPCTransportation` is actually Rain World's *within-room* creature-transport
  shortcut (never leaves the room) — the first fix's naming-based guess was wrong to seal it, and
  wrong to leave `CreatureHole` (a real den connection) unsealed.

  **Final, corrected fix:** `IsSealableExit` now seals `RoomExit` + `RegionTransportation` + `CreatureHole`
  ("room to room PIPES" and "creature DENS"), and leaves `Normal` + `NPCTransportation` unlocked
  ("within room SHORTCUTS" and "within room TRANSPORT for creatures"). The `FreePlayer`
  original-type-restoration fix from the first pass (`ChainState.ChainedShortcutOriginalTypes`, a
  `Dictionary<int, ShortcutData.Type>` remembering each sealed index's real type instead of
  hardcoding `RoomExit` on restore) carries over unchanged — still correct and still needed now that
  three types, not one, can get sealed. See "Chains gimmick, in detail" above for the full trace.
  Pushed the corrected `ChainsGimmick.cs` (68,902 bytes, verified byte-exact on the first attempt).
  **Update: user rebuilt and reported "normal shortcuts" still stayed locked.** Root cause was a
  second bug, not a mis-set `IsSealableExit`: `UpdateLock`'s belt-and-suspenders cancellation of
  in-progress shortcut entry (ported as-is from `CustomHRGuardManager.Update`) never checked *which*
  shortcut was being entered — it cancelled the player's and every tracked creature's shortcut entry
  unconditionally, every tick, for any shortcut in the room. Harmless in the source mod (which only
  ever seals `RoomExit`, so "entering a shortcut" and "entering a sealed shortcut" were the same
  thing there) but silently overrode the correct room-level type flip the instant this port started
  leaving `Normal`/`NPCTransportation` open on purpose. Fixed by adding a
  `state.ChainedShortcuts.Contains(...)` guard to both cancellation checks, so only entry into a
  shortcut this gimmick actually sealed gets cancelled. Pushed `ChainsGimmick.cs` again (70,330
  bytes, verified byte-exact on the first attempt).
  **Update: that guard didn't compile.** `dotnet build` returned CS1503 on both new lines --
  `cannot convert from 'RWCustom.IntVector2' to 'int'`. The guard had assumed
  `Creature.enteringShortCut` was a shortcut *index* (`Nullable<int>`, matching
  `state.ChainedShortcuts`'s own element type) without ever checking the real field -- wrong.
  Dumped `Creature`'s field signature straight from the game assembly's own metadata
  (`scratchpad/dnparse/dnmeta.py` + `sigdecode.py`) to get the compiler-true type:
  `Nullable<RWCustom.IntVector2>`, the tile being entered, not an index. Cross-checked
  `ShortcutData.StartTile`'s return type the same way -- also `IntVector2`, and confirmed
  `IntVector2` has `op_Equality` for a safe direct comparison. Fix: new
  `IsSealedShortcutTile(Room, ChainState, IntVector2)` helper walks `state.ChainedShortcuts` and
  compares each sealed index's `StartTile` against the given tile; both guard sites now call this
  instead of the mistyped `.Contains`. Pushed `ChainsGimmick.cs` a third time (71,638 bytes,
  verified byte-exact on the first attempt). See "Chains gimmick, in detail" above for the full
  trace. **Confirmed in-game by the user ("now it worked").** Room-to-room pipes and creature dens
  seal correctly; within-room shortcuts and creature-transport stay usable while locked.
  **Update (2026-09-13): full re-audit of the source mod at the user's request, four more findings.**
  Enumerated every type in `artificer_rubicon_hell_bonanza.dll` fresh and disassembled every
  not-yet-examined method rather than re-trusting earlier partial passes. Found: (1) **a real
  existing bug**, not just a gap -- `ChainsHuntedModeHook`'s room comparison was backwards, so
  "Faux-hunted burden" only ever instantly-spotted a player already sharing the creature's room
  (near no-op) instead of making different-room creatures hunt the player across the map, which is
  the entire point of the feature; fixed to match the source's actual control flow (bail out
  entirely on any colocated player, act on the first different-room player, then stop); (2)
  `GetCreatureList`'s initial room scan never excluded an already-friendly/tamed lizard (the check
  only existed in `TryAddArrivingCreature`, matching the source's own asymmetry between
  `GetCreatureList` and `AddNewCreature`) -- added the same exclusion to `GetCreatureList` rather
  than porting the source's whole belated 400-tick self-healing sweep that otherwise papers over
  this; (3) `IsArenaSession` guard, never ported -- the source disables itself entirely outside a
  story run, and this port's `PrimalSlot.IsActive` gate (a persistent save-slot option) doesn't
  automatically clear when switching from story into Arena/Sandbox, so this was a real, if narrow,
  gap; (4) a hardcoded `GW_A24` (Garbage Wastes) room exclusion, unexplained in the source's own
  code (like the VultureAI skip) but faithfully ported anyway as a simple "never lock in this room"
  check. Also re-confirmed several things already correct and needing no change: the
  `VultureAI.CreatureSpotted` gap remains a documented deliberate omission; the rest of
  `DoRedundancyCreatureCheck`'s repair logic isn't needed since this port's list management doesn't
  desync the way the source's does; and the options-registration/buffered-arrival-flush code already
  matches the source. Pushed `ChainsGimmick.cs` with all four fixes (77,586 bytes, verified
  byte-exact on the first attempt).
  **Update: user reviewed the four findings, rejected the GW_A24 one.** "remove hardcode on GW_A24
  / allow tamed lizards to not be chained." Tamed-lizard fix stays exactly as shipped, no change
  needed. `GW_A24`'s hardcoded room-name exclusion, despite being a faithful port of the source's
  own (unexplained) behavior, is something the user explicitly doesn't want in this mod -- removed
  outright, `TryLockRoom` now runs unconditionally on every room change again, comment left behind
  explaining what was there and why it came back out. `IsArenaSession` wasn't objected to and stays.
  Pushed `ChainsGimmick.cs` again (77,026 bytes, verified byte-exact on the first attempt). See
  "Chains gimmick, in detail" above for the full trace.
  **Not yet rebuilt or tested in-game** -- next: `dotnet build`, redeploy, and confirm (1) hunted
  mode actually makes an out-of-room creature path toward the player now; (2) a lizard already
  befriended before entering a room doesn't get chained; (3) Arena/Sandbox never triggers Chains;
  (4) Garbage Wastes room A24 now locks normally like any other room, no special-case exemption.
- **Confirmed working in-game (2026-09-13): Tab-to-switch-slot on the character-select screen,
  base mechanic.** Requested as a re-add of a previously-discarded idea, scoped narrowly to just
  the slot switch and explicitly asked to reuse the PRIMAL SLOT button's own switching machinery.
  New file `Gimmicks/PrimalSlotTabSwitchHook.cs` plus a small addition to `PrimalSlot.cs`
  (`LastRealSlot`) — see **Primal Slot Tab-switch, in detail** above for the full design. Built and
  tested first try with no fixes needed ("surprisingly, it works, well done").
  **Confirmed working in-game with 2 bugs (2026-09-13): vertical slide transition on top of the
  above.** Requested as a follow-up once the base switch was confirmed: entering Primal slides the
  screen up and off the top, then the rebuilt screen slides up into place from below; leaving
  Primal is the exact mirror (down and off the bottom, then down from above). Implemented by
  animating `Menu.Page.pos.y` directly, confirmed via disassembly to be the same
  `pos`/`lastPos`-interpolated mechanism vanilla's own UI already uses for smooth motion. User
  confirmed the buttons/foreground moved correctly, but reported two bugs: "the background didnt"
  move, and the motion should be "smooth... ease in ease out" rather than linear.
  **Confirmed NOT working (2026-09-13): round 2's background fix.** The user tested and reported
  "more easing, and the background still doesnt move." Root cause found by disassembling
  `Menu.SlugcatSelectMenu`'s constructor fresh: it never builds a `Menu.MenuScene` at all, so
  `menu.scene` is null for this menu and round 2's illustration-collecting code always had nothing
  to move — not a bug in the moving code, a wrong premise about where the background lives.
  **Confirmed working in-game (2026-09-13): round 3, replacing round 2's approach entirely.**
  Instead of chasing individual objects, disassembly of `Menu.MenuObject.get_Container()` showed
  every menu object's Futile container resolves recursively up to one shared `menu.container` (an
  `FContainer`) unless it or an owner overrides it — and nothing in `Page` or the base `Menu` class
  ever does. So `menu.container` is the single node everything the menu draws is parented under,
  regardless of mechanism, and round 3 animates `container.y` directly instead of collecting a list
  of objects. `Menu.container` itself is fetched via a new `AccessTools.Field` lookup
  (`MenuContainerField`) per this file's own GOTCHA #1 discipline; `FContainer.y` itself needs no
  such guard, being the same `FNode`-inherited property already used directly for the black screen.
  For "more easing," swapped the sinusoidal ease for a cubic ease-in-out (`4t³`/mirrored) and raised
  `SlideDurationTicks` from 16 to 30 (~0.75s). User confirmed: "good, now it works." See "Primal
  Slot Tab-switch, in detail" above for the full trace.
  **New, pushed, not yet tested in-game (2026-09-13): round 4, preserving the selected character
  across the switch.** Immediately after confirming round 3, requested: "when switching, it doesnt
  switch between most recent in both save files but switches to the same character -- so if you are
  on primal artificer, you switch to normal artificer. if you are on normal gourmand you switch to
  primal gourmand." Disassembly found `SlugcatSelectMenu` tracks the shown character as a single
  index (`slugcatPageIndex`) that only auto-restores from save data when a slot has exactly one
  existing character, defaulting to index 0 otherwise — not "most recently played" as it appeared.
  Fixed by capturing the character showing on the OLD menu when Tab is pressed
  (`GetSelectedSlugcat`), then finding and pointing at the same one on the rebuilt menu
  (`ApplyCharacterFix`), with the black screen held an extra fixed handful of frames
  (`RevealSettleFrames`) as cheap insurance against per-page position state that updates
  incrementally rather than instantly. Six more previously-untouched fields now go through
  `AccessTools` per GOTCHA #1. See "Primal Slot Tab-switch, in detail" above for the full trace.
  **New, pushed, not yet tested in-game (2026-09-13): round 5, multiple Primal slots, requested
  immediately after round 4 and before round 4 was ever tested.** "instead of switching between
  save slot 1/2/3 and primal slot (666666) lets switch between slot 1 and primal slot 1 (666666)
  then when in slot 2 you switch between slot 2 and primal slot 2 (666667) and so for slot 3" — one
  shared reserved slot becomes one reserved slot per real slot (`ReservedSlot + realSlot`, via new
  `PrimalSlot.PrimalSlotFor`/`RealSlotFor`, replacing the old single `LastRealSlot` variable
  entirely), with `PrimalSlot.IsActive` changed from exact equality to a range check
  (`saveSlot >= ReservedSlot`) so it still recognizes any of them. The Options-menu "PRIMAL SLOT"
  button (`PrimalSlotOptionsMenuHooks.cs`) got the equivalent fix for consistency — it now maps to
  whichever real slot is currently mounted's own Primal counterpart instead of always targeting
  666666. See "Primal Slot Tab-switch, in detail" above, "Round 5, multiple Primal slots," for the
  full trace. `PrimalSlot.cs` (5,294 bytes), `PrimalSlotTabSwitchHook.cs` (54,684 bytes), and
  `PrimalSlotOptionsMenuHooks.cs` (9,742 bytes) all pushed together and verified byte-exact on the
  first attempt. **None of round 4 or round 5 has been built or tested in-game yet** — next:
  `dotnet build`, then in-game confirm (1) each real slot's Tab press lands on its OWN distinct
  Primal slot (666666/666667/666668), not a shared one; (2) Tab back out from each returns to the
  correct originating real slot; (3) character preservation (round 4) still works per slot, in both
  directions, for more than one character; (4) the Options-menu "PRIMAL SLOT" button mounts the
  right slot-specific Primal save from each of the 3 real slots and shows as selected whenever any
  Primal slot is active; (5) the round-3 slide transition still plays correctly; (6) check
  `BepInEx/LogOutput.log` for any `PrimalSlotSwitcher`/`PrimalSlot` line.
- **New, built, not yet built by the user or tested in-game (2026-09-12): NoYeeks, NoCicadas,
  NoMauling.** Requested as "no grabbing yeeks, no grabbing squidcadas and no mauling" — three plain
  on/off gimmicks (no settings), all default-assigned to Artificer. See "NoYeeks, NoCicadas, and
  NoMauling gimmicks, in detail" above for the full trace, including the disassembly-confirmed fact
  that `Player.CanMaulCreature` is already Artificer-exclusive in vanilla via
  `SlugcatStats.SlugcatCanMaul`. Pushed all five touched files (`NoYeeksGimmick.cs` 2,233 bytes,
  `NoCicadasGimmick.cs` 2,362 bytes, `NoMaulingGimmick.cs` 2,442 bytes, `GimmickRegistry.cs` 8,477
  bytes, `GimmickDefaults.cs` 3,984 bytes) and verified all five landed correctly on the first
  `device_commit_files` attempt (no silent-write retry needed this time). Next: `dotnet build`, and
  if clean, confirm in-game per the checklist in that section above.
- **New, built, not yet built by the user or tested in-game (2026-09-12): Inv added to the
  roster, and NoPassages.** Two asks in one message: (1) "add inv to the selection menu and give
  him those gimmicks along with nospear" — added `MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel` to
  `PrimalOptions.AllCharacters` as "Inv" (web-searched to confirm "Inv"/"Sofanthiel" really
  is More Slugcats' secret slugcat rather than guessing — see "Config menu UI, in detail" above),
  and default-assigned NoYeeks/NoCicadas/NoMauling/NoSpears to it in `GimmickDefaults.cs`; (2) a new
  "No Passages" gimmick — Passages stay visible on the cycle-complete/death screen but their button
  is disabled via the real vanilla `ButtonBehavior.greyedOut` flag (see "NoPassages gimmick, in
  detail" above for the full disassembly trace), registered with no default assignment. Pushed all
  four touched files (`PrimalOptions.cs` 23,087 bytes, `GimmickRegistry.cs` 9,347 bytes,
  `GimmickDefaults.cs` 4,706 bytes, `NoPassagesGimmick.cs` 4,915 bytes) and verified all four landed
  correctly on the first `device_commit_files` attempt. Next: `dotnet build`, and if clean: confirm
  Inv now appears as a 10th button in the character grid with the right four gimmicks pre-assigned,
  and confirm NoPassages per the checklist in its own section above (assign it to some character
  first via the Remix menu, since it has no default).
- **New, built, not yet re-tested by the user (2026-09-12, three rounds — see Gotcha #7 above for
  the full trace of all three): gimmick-assignment persistence fix.** Reported as "on game reset,
  gimmicks dont get saved on which characters they got applied to."
  **Round 1** fixed the *load* side: this mod's own config-binding only ever ran once the player
  opened its Remix page, but the real game's automatic config-load pass (`RainWorld.PostModsInit`)
  runs earlier than that on every launch and silently skips any OptionInterface with nothing bound
  yet. Fixed by splitting the bind loop out into a new idempotent
  `PrimalOptions.EnsureConfigBound()`, called eagerly from `RainWorldHooks.Postfix` (during
  `OnModsInit`, before `PostModsInit` fires) in addition to its existing lazy call from
  `Initialize()`. **The user tested this and reported it still didn't work** — "dotnet build,
  copied files, added no passages to inv, reset game, no change. The game did not however give me
  an option to apply the saved config of the remix settings." That surfaced a second, more
  fundamental bug round 1 had wrongly assumed away.
  **Round 2** attempted to fix the *save* side: Remix's per-mod config screen has its own dedicated
  native "SAVE" button (separate from just backing out of the screen), which this mod's own
  full-screen character-grid UI gives no indication of and likely crowds out entirely. Fixed (or so
  it seemed) by having `PrimalOptions.SetEnabledFor()` (the single choke point both
  `AddGimmick`/`RemoveGimmick` already go through) call `config.Save()` immediately and
  unconditionally on every assignment change. **The user tested this too and reported it still
  didn't work** — "still dont work man, rebuild copy, open game, add no passage to inv, reopen, did
  not save."
  **Round 3** found the actual, definitive root cause via direct evidence rather than more
  disassembly theory: checked the user's real `ModConfigs` folder on disk (zero bytes ever written —
  no config file for this mod existed at all) and checked BepInEx's `LogOutput.log` (which had the
  exact answer already logged by round 2's own try/catch): `config.Save()` was throwing
  `System.MethodAccessException` on every single call, because `ConfigHolder.Save()` is genuinely
  non-public at runtime despite showing public flags in this project's compile-time reference
  assembly (the same "publicized reference assembly lies about accessibility" class of bug as
  Gotcha #1, now confirmed to apply to methods as well as fields). Fixed by calling it through
  reflection instead — `AccessTools.Method(typeof(ConfigHolder), "Save").Invoke(config, null)` —
  the same pattern this codebase already used defensively for non-public fields elsewhere. Pushed
  `PrimalOptions.cs` (33,612 bytes, round 3) and verified it landed correctly on the first
  attempt (no retry needed). **Not yet built or tested** — next: `dotnet build`, copy the DLL, then:
  assign/unassign a gimmick, fully quit and relaunch the game *without* opening Remix again, confirm
  the assignment held, **and this time also check `...\Rain World\ModConfigs\rokas.primal.txt`
  directly** to make sure it now exists and contains the assignment, rather than relying on in-game
  behavior alone. Given this is the third attempt, treat "should work now" with real skepticism
  until actually confirmed — this round at least has harder evidence behind it than the previous
  two (an exact exception trace plus a definitively-empty config folder, not just a disassembly
  read), but "should" still isn't "confirmed."
- **Needs re-test from the user:**
  - Spearmaster's grab/spear-pull, to confirm the `FieldAccessException` fix actually resolved the
    freeze (high confidence it does, based on the exception trace, but unconfirmed as of this doc).
  - The rebuilt Misery gimmick on Gourmand, to confirm it now matches the reference mod's actual
    harshness (also unconfirmed as of this doc).
  - TimeLimit and NoSpears on Spearmaster (both implemented, never run in-game) — see "TimeLimit and
    NoSpears gimmicks, in detail" above for what to check if either doesn't seem to work.
  - ~~Character grid 5x2 overflow fix and the Gotcha #4 `MultiuseConfigurableException` fix~~ —
    **confirmed resolved by the user.**
  - ~~The character-grid/detail-panel UI merge~~ — **tested, did not work as designed (behaves
    like the old drill-down); user asked to leave it alone.** Do not resume unprompted — see
    "Config menu UI, in detail" above for the full report and an unconfirmed hypothesis.
  - **New, highest priority:** `PrimalCharacterNames.cs` (see "Primal character names, in detail"
    above) — has compiled clean in every build since it was written, so just needs in-game
    confirmation: on the alt slot, Gourmand, Spearmaster, Artificer (2026-09-12), and Sofanthiel
    (2026-09-15) show "PRIMAL GOURMAND"/"PRIMAL SPEARMASTER"/"PRIMAL ARTIFICER"/"PRIMAL ???" with a
    description listing their currently-assigned gimmicks (Sofanthiel's will just read the generic
    placeholder, since it has none); **and (added later the same day) Survivor, Monk, Hunter,
    Rivulet, Saint, and Watcher show "PRIMAL SURVIVOR"/"PRIMAL MONK"/"PRIMAL HUNTER"/"PRIMAL
    RIVULET"/"PRIMAL SAINT"/"PRIMAL WATCHER" with the literal static description "Primal version not
    implemented"** — on the "choose your character" (new-game) screen; every one of these ten
    characters off the alt slot is unaffected; and toggling a gimmick in Remix changes the wording
    next time that screen is shown for the four characters whose description is gimmick-driven. As
    of 2026-09-15 Gourmand's and Spearmaster's intro lines are the user's own rewrites (see "Primal
    character names, in detail"), not the original generic "changed by something primal" placeholder
    text.
  - **New, highest priority:** `PrimalUnimplementedPortraits.cs` (see "Primal character portraits,
    in detail" above) — the six "not implemented" characters' select-screen portraits should render
    as solid black silhouettes on the alt slot, with everything else (background/vines/pipes/
    plants, the other four characters' own portraits, every character off the alt slot) unaffected.
    Specifically worth checking whether the three base-game portraits (Survivor/Monk/Hunter, whose
    shader depends on whether the MMF mod is installed) black out correctly the same as the three
    MSC ones (Rivulet/Saint, always `MenuShader.Basic`) — see that section's own doc comment for
    why this could plausibly diverge and how to tell if it does.
  - Master Spear's (OneSpear's) now-configurable pull limit — confirm raising it above 1 in the
    settings sub-view actually grants more than one pull, and that the save-marker format change
    doesn't break anything on a save file that already has old-format markers on it (see the
    file-map entry for `OneSpearGimmick.cs` above).
- **Not yet done:** no gimmick has been implemented for characters other than Gourmand (Misery +
  Slugpup), Spearmaster (OneSpear + TimeLimit + NoSpears), and Artificer (Chains, added 2026-09-12)
  — Rivulet, Saint, Watcher, White/Yellow/Red all have Remix checkboxes available but no default
  assignment and no gimmick concept designed yet. This may or may not be an intended next step —
  hasn't been discussed.
- **Not yet done:** TimeLimit's game-over presentation. Currently just an instant kill with no
  special screen or message — the user explicitly deferred this ("Later on we can implement some
  sort of game over screen, for now if timer > 1hour = kill character that tick"), so don't build
  it unless asked.
- **New, built, not yet built by the user or tested in-game (2026-09-13): Threat Slowdown.**
  Requested as "as the threat pulse increase, the character slows down," tying the effect to
  vanilla's own "Threat Music Pulse" Remix option and the user's own description of its 5-circle
  visual ("threat level 1" = one full circle). Reverse-engineered the real raw threat value
  (`ProcessManager.musicPlayer.threatTracker.currentMusicAgnosticThreat`, with vanilla's own
  fallback) and the exact numeric threshold for "one full circle" (`1.1f / 5f == 0.22`) from
  `ThreatPulser`'s own disassembled rendering code, rather than guessing. Scales
  `runspeedFac`/`poleClimbSpeedFac`/`corridorClimbSpeedFac`/`swimForceFac` linearly 100%→0% over
  that range, and (since `throwingSkill` is inverted — 0 is vanilla's BEST accuracy) ADDS a scaled
  penalty (`+0..3`) instead. New file `Gimmicks/ThreatSlowdownGimmick.cs` (18,774 bytes) plus a new
  `GimmickDef` in `GimmickRegistry.cs` (10,386 bytes) — no default character assignment, not
  requested. See "Threat Slowdown gimmick, in detail" above for the full trace. Both pushed and
  verified byte-exact on the first attempt. **Not yet built or tested** — next: `dotnet build`,
  assign the gimmick to a character via Remix, then in-game confirm movement/climbing/swimming
  visibly slow as threat rises, throws visibly get less accurate over the same ramp (and whether
  the `3`-point max penalty feels right), stats fully restore with no drift once threat eases off
  even after repeated spikes, the effect works whether or not "Threat Music Pulse" is turned on in
  Remix, and check `BepInEx/LogOutput.log` for any `ThreatSlowdownGimmick` line.
  **Update (2026-09-13): confirmed triggering correctly in-game**, after one false alarm — user
  first reported "no effect even at 2.5 circles filled," which turned out to be testing on a
  non-Primal save slot (every gimmick in this mod requires `PrimalSlot.IsActive`); once on the
  correct slot, "ok, my bad, it did work."
- **New, built, confirmed working in-game (2026-09-13): Threat Panic.** Requested once Threat
  Slowdown's trigger was confirmed working: "as it approaches .22, monk starts panicing... take
  away more and more control from the player, disallowing input." Investigated slugpup/creature
  "panic" AI as a reference point and found nothing reusable for a player-controlled character (see
  "Threat Panic gimmick, in detail" above for the full trace of `Player.controller`/`Player.AI` and
  why neither offers a graduated version of this); instead, Postfixes `Player.checkInput()` and
  probabilistically zeroes that tick's already-assembled `Player.input[0]` — 0% chance at no
  threat, 100% (full lockout) right at threat level 1 — sharing `ThreatSlowdownGimmick`'s own
  threshold/lookup rather than duplicating it. New file `Gimmicks/ThreatPanicGimmick.cs` (12,023
  bytes); `GimmickRegistry.cs` (11,417 bytes) and `GimmickDefaults.cs` (5,186 bytes) updated —
  defaulted onto Monk per the request, alongside (not replacing) Threat Slowdown. All four touched
  files pushed and verified byte-exact on the first attempt. **User has since tested and confirmed
  it works** — no further in-game verification outstanding for the base mechanism, but the *feel*
  of it (dropped-input stutter vs. something more like erratic/random movement) hasn't been
  specifically commented on — worth asking about if it comes up again.
- **New, built, not yet built by the user or tested in-game (2026-09-13): add-gimmick picker made
  scrollable.** Reported right after Threat Panic was confirmed working: "add a scrollwheel to the
  gimmick picking list, because you cant see all of them at this point" — with 12 gimmicks now
  registered, `RenderAddGimmick`'s list could need up to 12 rows, well past the ~300px actually
  available. Fixed using the game's own `Menu.Remix.MixedUI.OpScrollBox` (traced its real
  constructor/usage from the base game's own `OptionInterface.ErrorScreen` via a raw IL search —
  see "Add-gimmick picker made scrollable" under "Config menu UI, in detail" above for the full
  trace). Pushed `Gimmicks/PrimalOptions.cs` (37,213 bytes) and verified byte-exact on the first
  attempt. **First rebuild crashed immediately on opening the "+" picker:**
  `System.InvalidOperationException: OpScrollBox must be added to an OpTab before items are
  added.` — `RenderAddGimmick` had populated the scroll box's own rows (`scrollBox.AddItems(...)`)
  before the scroll box itself had been committed to the `OpTab` via `ShowDetail`, and that
  ordering turns out to be a hard requirement, not just a suggestion. Fixed same day by calling
  `ShowDetail(items)` right after adding the still-empty scroll box (attaching it to the `OpTab`
  first), then populating its rows afterward — see "Add-gimmick picker made scrollable" above for
  the full account. Pushed again (37,882 bytes), verified byte-exact. **Not yet rebuilt or
  re-tested** — next: `dotnet build`, open the Remix menu, pick a character with few/no gimmicks
  assigned, open its "+" picker, and confirm it opens without the error screen this time, all 12
  gimmicks are reachable (scroll wheel and/or the slide bar), the last one in the list is clickable
  and actually assigns correctly, and nothing else on the screen (character grid, assigned-gimmick
  table, settings sub-views) regressed.
- **New, not yet built or tested (2026-09-13): Threat Slowdown / Threat Panic swap.** Once the
  input-drop mechanic (then called "Threat Panic") was confirmed working, the user asked: "rename
  the gimmick that drops inputs on threat to threat slowdown and make the threat panic throw random
  inputs. logic should go like this - choose input between 4 directional grab throw jump, and then
  keep it for 1-10 frames. Frequency sets how often these happen. Allow player to always input.
  Inputs can overlap." Implemented as a code swap between the two files rather than an ID/display
  rename (see "Threat Slowdown and Threat Panic gimmicks, in detail" above for the full account and
  why): `ThreatSlowdownGimmick.cs` (17,083 bytes) now has the input-drop mechanic (unchanged
  mechanically from its old "Threat Panic" implementation); `ThreatPanicGimmick.cs` (17,852 bytes)
  now has a brand new mechanic — random forced inputs (4 directions + grab + throw + jump, held
  1-10 ticks, frequency configurable per character via the new `PanicSettings`, real input never
  blocked, multiple forced inputs can overlap); `GimmickRegistry.cs` (12,716 bytes) had both
  `GimmickDef` descriptions rewritten and wired `ThreatPanic` up to its new settings.
  `GimmickDefaults.cs` needed no change (Monk's existing default assignment is by ID, which didn't
  change). All three files were pushed via `device_commit_files` and `device_list_dir` confirmed
  each one's size/mtime actually changed to a new, non-stale value — but this container's Bash tool
  had an intermittent safety-classifier outage during this round, so the usual local
  `wc -c`-before/`device_list_dir`-after exact-byte-count comparison couldn't be completed as
  rigorously as normal; treat this round's push as very likely correct but not double-verified the
  usual way, and re-check closely if anything about either gimmick seems off. **Not yet built or
  tested** — see the checklist at the end of "Threat Slowdown and Threat Panic gimmicks, in detail"
  above.
- **New, not yet built or tested (2026-09-15): per-gimmick settings sub-view made scrollable.**
  Reported directly: some gimmicks' settings sub-view — Chains on Artificer, notably — don't fit
  the available space. `GimmickDef.cs`, `GimmickRegistry.cs`, and `PrimalOptions.cs` all touched;
  see "Per-gimmick settings sub-view made scrollable" under "Config menu UI, in detail" above for
  the full account. This session's container has no `.NET` toolchain at all (see "Container
  environment notes" below), so none of this could be compiled or run here — written by close
  analogy to the already-shipped, already-working add-gimmick-picker scroll box, but genuinely
  unverified. Next: `dotnet build`, then in-game confirm (1) it compiles; (2) Chains' settings for
  Artificer now scroll to reach every control (escape timer, both toggles, preset picker, all 12
  banlist checkboxes) instead of clipping; (3) every other gimmick's settings sub-view
  (TimeLimit/NoSpears/OneSpear/ThreatPanic) looks and behaves completely unchanged; (4) no visual
  collision between the new scroll box's slide bar and anything at the settings screen's right edge.
- **New, not yet built or tested (2026-09-15): gimmick buttons colored by home character.**
  Requested directly: color-code each gimmick's button by whichever character it's thematically
  tied to (Chains ↔ Artificer, ThreatPanic ↔ Monk, etc.), using that character's own vanilla color.
  `GimmickDef.cs`, `GimmickRegistry.cs`, and `PrimalOptions.cs` all touched; see "Gimmick buttons
  colored by their home character" under "Config menu UI, in detail" above for the full account,
  including why `PlayerGraphics.DefaultSlugcatColor` was used over the already-present
  `SlugcatColor` reflection call in `SlugpupGimmick.cs`. Same container limitation as the entry
  above (no `.NET` toolchain here to compile/test against) — both new member accesses
  (`OpSimpleButton.colorEdge`, `PlayerGraphics.DefaultSlugcatColor`) were confirmed against
  `reference/GameSource` rather than guessed, and the reflection call is wrapped in its own
  try/catch specifically so a failure here can't crash the whole Remix screen open, but none of it
  has actually run yet. Next: `dotnet build`, then in-game confirm (1) it compiles; (2) every
  gimmick's button (in both the assigned-gimmicks table and the add-gimmick picker) shows its home
  character's real color as a border+text tint, and NoPassages' button looks completely default/
  untinted; (3) tinted text stays legible against the button's black fill for every character,
  especially darker ones (Spearmaster, Sofanthiel/Inv); (4) check `BepInEx/LogOutput.log` for a
  `PlayerGraphics.DefaultSlugcatColor threw` line -- if present, every button fell back to
  uncolored and the reflection lookup needs a fix.
- **New, not yet built or tested (2026-09-15, revised four times same day): Kill List gimmick.**
  Requested as a completionist tracker -- kill every one of a fixed, curated 55-species roster,
  tracking only for now (no goal/reward yet). New file `Gimmicks/CreatureKillListGimmick.cs`;
  `GimmickRegistry.cs` updated (no default assignment, no home-character color tag, no settings
  sub-view at all -- not requested). See "Kill List gimmick, in detail" above for the full account.
  Round 5 ("your list doesnt include the small spiders"): a real curation miss caught by the user --
  plain `CreatureTemplate.Type.Spider` (distinct from the already-tracked `BigSpider`/`SpitterSpider`)
  was silently dropped during the very first curation pass instead of being flagged as unmatched.
  Confirmed via `Spider.cs` (each spider belongs to a `Flock`/`SpiderMass`, periodically merging with
  others via `Merge(SpiderMass otherFlock)`) rather than guessed, then added as
  `new KillListEntry(CreatureTemplate.Type.Spider, "Small Spider")` per the user's chosen display
  name -- 54 entries up to 55, every roster-count reference elsewhere updated to match.
  Round 4: `Yeek` and `SlugNPC` ("Slugpup") removed from the roster outright, no reason given --
  56 entries down to 54, `KillListRegistry.All` and every roster-count reference elsewhere updated
  to match.
  Round 2: (1) kills are now only banked permanently if the cycle actually ends in a successful
  sleep (`RainWorldGame.Win`, confirmed to fire only from `ShelterDoor.cs`) -- a cycle ending in
  death (`RainWorldGame.GameOver`, confirmed to fire only from real death paths) discards that
  cycle's kills instead; (2) progress is no longer shown in the Remix menu at all -- instead a
  "KILL LIST" button on the cycle-end screen (`Menu.SleepAndDeathScreen`) opens a popup listing
  not-yet-killed species, then killed. Round 2 also traced attribution in full (13 total
  `SetKillTag(` call sites) and flagged, but didn't fix, a suspected throw-impact gap.
  Round 3 ("double check the leech kill path too") **confirmed and fixed a real gap**: `Leech.cs`
  isn't `IPlayerEdible`, so the "grab and eat" attribution path never applies to it at all, and its
  actual, everyday death mechanism -- an `airDrown` (out-of-water) timer that calls `Die()` directly
  with zero `SetKillTag` anywhere in that path -- would never have been counted. Fixed with a new,
  general fallback (`CreatureKillListHandlerHook`, a Postfix on `PhysicalObject.Grabbed`) that
  records "who last grabbed this creature" and lets `Die()`'s Postfix use that instead, but only
  when `killTag` is entirely absent (never overriding a `killTag` that's set to something else).
  Same container limitation as the entries above (no `.NET` toolchain here) -- every new member
  access was checked against `reference/GameSource/` rather than guessed, but none of it has
  compiled or run once. Next: `dotnet build`, then in-game, in order: (1) it compiles; (2) assign
  Kill List to a character, kill a tracked species by grabbing and eating it, reach a shelter, and
  confirm the "KILL LIST" button on the sleep screen shows it under KILLED; (3) kill a *different*
  species, then deliberately die before reaching a shelter, and confirm that cycle's kill was
  discarded; (4) fully quit/relaunch, reload the same character, reach a shelter, and confirm the
  earlier banked kill survived; (5) **highest priority: pull a leech off, throw it somewhere dry,
  wait ~22.5 seconds for it to die of `airDrown`, and confirm it's still attributed correctly** --
  this is the specific gap this round fixed; (6) test a throw-into-wall kill for some other species
  now that the handler-fallback exists, and a slow multi-second spear-stab kill (never grabbing the
  target directly), to check the two attribution gaps still flagged as open; (7) kill something NOT
  on the list (e.g. a Watcher creature) and confirm nothing is recorded and nothing throws; (8)
  confirm the "KILL LIST" button only appears for a character that actually has the gimmick
  assigned.

## Container environment notes

**2026-09-11 session: this container had no .NET toolchain at all** — no `dotnet`, no `mono`, no
`mcs`/`csc`. Confirmed via `which`/`find`; not something fixable by installing, either: `apt-get
update` gets `403 Forbidden` on every Ubuntu mirror (archive.ubuntu.com, security.ubuntu.com), and
`curl https://dot.net/v1/dotnet-install.sh` gets rejected by the egress proxy
(`connect_rejected`/403) — confirmed via `curl "$HTTPS_PROXY/__agentproxy/status"`, whose `noProxy`
allowlist covers `registry.npmjs.org`, `pypi.org`, `files.pythonhosted.org`, `index.crates.io`,
`proxy.golang.org`, and Anthropic's own API hosts, but nothing .NET/NuGet/Ubuntu-related. This is a
**different, more restrictive** environment than whatever container the earlier "builds clean,
0 errors" sessions had (this doc previously stated builds as already-verified fact without
flagging this risk — that assumption doesn't always hold and should be checked early each session,
not assumed).

**Practical effect:** the UI merge described above in "Config menu UI, in detail" could only be
hand-reviewed against the existing code's own conventions (API usage patterns copied from the parts
of `PrimalOptions.cs` that were kept unchanged, since those were already known to compile)
and committed straight to source — it was **not** run through `dotnet build` before being deployed.
If a future session also finds no toolchain available, either ask the user to build locally
themselves (they've done this before — the same `dotnet build` command below runs fine on their own
Windows machine) and report back with any compiler errors, or check whether that particular
container instance happens to have `dotnet`/`mono` before assuming either way. The Slugpup stats
investigation (see "Slugpup gimmick stats rescaling, in detail") shows exactly what this
limitation costs in practice: a real, run-of-the-mill nested-type name error (`Player.NPCStats` vs.
bare `NPCStats`) that `dotnet build` would have caught in under a second went undetected through an
entire push-and-ask-the-user-to-test round trip, because there was no way to compile-check it
first. Where possible, prefer verifying a type/member's real name and shape against the reference
assembly's own metadata (see the `dnmeta.py` paragraph below) over trusting that a plausible-looking
reference works, rather than relying solely on the user's own `dotnet build` to catch it.

**No .NET decompiler is installable here either** (same session, same investigation): `ikdasm`,
`ilspycmd`, `mono`, `monodis` are all absent, and `pip3 install` fails for every package tried —
not just decompiler-specific ones (a plain, definitely-real package like `cowsay` fails identically)
— despite `pypi.org`/`files.pythonhosted.org` nominally being in the proxy's allowlist. Concluded
this is a genuine, currently-unresolved network restriction rather than retrying indefinitely.
**Built instead: a small, reusable, pure-stdlib-Python ECMA-335 PE/metadata parser and CIL
disassembler**, at
`scratchpad/dnparse/dnmeta.py` in whichever session investigated this (a cloud-container scratch
path, not part of this repo — re-locate or rewrite it fresh if a future session needs it and can't
find it, ideally saving a copy somewhere durable like this repo's own tooling folder next time).
Given any `Assembly-CSharp.dll`-shaped file, it exposes `DnAssembly(data)` with `.get_string(idx)`,
`.get_us(idx)` (user-string heap), `.tables[table_id]` (raw metadata rows, e.g. `0x02`=TypeDef,
`0x04`=Field, `0x06`=MethodDef, `0x29`=NestedClass), `.read_method_body(methodRow)` (raw IL bytes
from an RVA), and `.resolve_token(token)`; a module-level `disassemble(code, asm)` turns raw IL
into readable `IL_xxxx: mnemonic operand` lines with resolved call/field targets and string
literals. This is what found every fact in "Slugpup gimmick stats rescaling, in detail" above —
Pupify's exact stat-scaling constants, `NPCStats`'s randomized-personality logic,
`PlayerGraphics.ApplyPalette`'s color branching, and finally the `NestedClass` table lookup that
caught the `Player.NPCStats` naming issue. Worth reaching for again any time a vanilla behavior
needs to be understood exactly rather than guessed at from a publicized reference assembly's
(sometimes misleading) surface shape, or any time a third-party mod's own logic needs
reverse-engineering the way Pupify's was here.

## Build & deploy workflow (for this container / a future one like it)

```
cd src/Primal
dotnet build            # net48, builds clean against lib/*.dll (see .csproj) -- when a toolchain
                         # is actually available in this container; see "Container environment
                         # notes" above if it isn't.
```

Output: `bin/Debug/Primal.dll`. To ship a change to the user's machine (this container has
no direct access to it — everything goes through `SendUserFile` + the remote-devices bridge):

1. Copy the built DLL to `/mnt/user-data/outputs/Primal.dll` (and any changed `.cs` files,
   for keeping the user's own source tree in sync).
2. `SendUserFile` on those paths to get `file_uuid`s.
3. `device_commit_files` each to its real device path:
   - DLL → `C:\Program Files (x86)\Steam\steamapps\common\Rain World\RainWorld_Data\
     StreamingAssets\mods\Primal\plugins\Primal.dll`
   - Each changed `.cs` → its mirrored path under
     `C:\Users\rokas\Documents\RWMods\src\Primal\...`

There is no git repo here (`git` was never initialized in this working tree) — treat the source
tree as the single source of truth on the user's own machine; this container's copy of it is
whatever was staged in for the current session and is not authoritative between sessions.

If diagnosing a "silently doesn't work" report: stage and read, in this order,
`BepInEx\LogOutput.log` (does the mod even show a "loaded" line? any `LogError`?), then
`exceptionLog.txt` / `consoleLog.txt` at the game root (per-tick exceptions the game's own update
loop swallows without crashing, invisible to BepInEx's log) — **before** re-reading this mod's own
code looking for the bug. Both prior major "invisible" bugs in this mod (Gotchas #1 and #2) were
found this way, not by code review.

#HUMAN WRITTEN ISSUES FOUND DURING TESTING
Gourmands Misery gimmick also implements spawning yeeks on room entrance - this is not how it should work, only the exhaustion should be a part of the gimmick
Gourmand does not grow up after visit to OE ending
New Gimmick - Food. Sets how much food you the character needs to eat - one value sets minimum, other maximum. Give to gourmand, default min to 7, max to 11
Spearmaster by default has the death timer set to per run, but after sleeping in a shelter and waiting for a minute with a minute death timer, I died after a minute. And after that, respawning didn't instantly kill me, had to wait another minute - seems cycle based, not whole run, including deaths, based
Saint does not need the inert until cycle 1 gimmick anymore - default to 0, but keep the functionality, since it works
No Yeeks and No Squidcadas gimmicks are now useless, superceded by No Hold
Chains shouldn't have visuals coming from creature dens - only from slugcat enterable pipes and shortcuts
