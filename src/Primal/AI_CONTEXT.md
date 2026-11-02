# Primal (Rain World mod) — context for picking this back up

Read this whole file before touching code. It exists because this project has gone through two
major architecture pivots and several misdiagnosed bugs, and re-deriving that history from the
code alone wastes time. Originally written 2026-09-11; reorganized and trimmed 2026-11-02 (see
"About this reorganization" at the very bottom for what changed and why, and where the old
day-by-day history actually went).

**Deferred, not forgotten — flagged to the user, explicitly deferred by them (2026-11-02):**
Rivulet's shelter-visit requirement has a known issue the user wants investigated eventually
("Rivulet shelter permanent failure") but explicitly asked NOT to be worked on right now ("your
goal is not to do rivulet"). Don't pick this up unprompted — ask first. See "Known issues from
real testing" below for what's actually known about it.

## Maintenance policy — keep this document current

**This file is a living document, not a one-time snapshot.** Whoever (whichever session) is
working on this mod should update it as you go, not just when explicitly asked to summarize:

- After any architecture decision, pivot, or discarded approach — add it to History so it isn't
  re-litigated later.
- After root-causing any bug, especially a "silent"/hard-to-diagnose one — add it as a Gotcha, with
  the symptom, the actual cause, and how it was found (not just the fix), so the same investigation
  isn't repeated from scratch.
- After adding, removing, or substantially changing a file — update the file map.
- After the user reports a test result (confirmed working, still broken, wrong behavior) — update
  **Current status** so the next session knows current status without re-reading the whole
  conversation history.
- Before a conversation is likely to end (context running low, task paused, user says they'll test
  and report back later) — do a pass over this whole file to make sure it still matches reality.

**Keep it accurate over keeping it short — but don't duplicate what a `.cs` file's own header
comment already says.** Every gimmick/requirement file in this codebase carries a thorough doc
comment at its own top with the real implementation trace (reverse-engineering notes, exact vanilla
mechanisms, why a given approach was chosen). This file's job is the *cross-cutting* picture a
single source file can't carry: current status across the whole mod, lessons that apply to more
than one file (Gotchas), major pivots, and pointers into the source — not a second copy of each
file's own comment. When adding detail about one specific gimmick's mechanism, prefer putting it in
that gimmick's own `.cs` file and linking to it here, over growing this file's own prose. Prune
stuff that's no longer true rather than leaving stale info. This instruction itself should stay in
the document — don't remove this section when editing.

## What this is, in plain terms

Rain World is a Unity game moddable via BepInEx + Harmony. This is a BepInEx plugin, C#, targeting
.NET Framework 4.8, built with `dotnet build` (MSBuild), using Harmony to patch the game's own
compiled methods at runtime (no source access to the game itself — only a decompiled/"publicized"
reference DLL to compile against, see **Gotcha #1** below, which matters a lot).

**The feature, from the player's perspective:** the Options menu gets a 4th save-slot button,
"PRIMAL SLOT", alongside vanilla's 3 normal save slots (one Primal counterpart per real slot — see
Current architecture). Switching to it is a completely normal save-slot switch (same vanilla code
path, same busy-spinner, same failure/revert handling — this mod does not reimplement any of that).
On that slot, and only that slot, real vanilla characters can have extra "gimmicks" layered on top
of them — small gameplay modifiers like "Gourmand gets exhausted much more easily" or
"Spearmaster's iconic tail-spear-pull only works a limited number of times" — plus a separate
"Requirements" system tracking run-completion conditions (including the Rain World community's own
50-challenge list). Off that slot, every character plays 100% unmodified vanilla.

**Important: there are no custom characters.** Two earlier designs tried creating new SlugBase
characters (clones of the vanilla roster) to carry the alt-mode identity. Both were scrapped by
explicit user decision — see **History** below. The current, correct design applies gimmicks
directly to the real vanilla `Player` object, gated on which save slot is mounted. If you ever find
yourself wanting to add a new SlugBase character or a "roster" concept back in: don't. That was
tried twice and rejected both times for good reasons.

## History — major pivots and discarded designs

- **Design #1 and #2 (both discarded): custom SlugBase characters.** Two earlier attempts built 9
  new SlugBase characters (clones of the vanilla roster) to carry the alt-mode identity, plus a
  character-select roster filter and forced session-identity hooks. Both were rejected by explicit
  user decision in favor of the current design (gimmicks applied directly to real vanilla
  characters, gated by save slot). Every trace of both designs (`Characters/`, `CharacterSelect/`,
  `Gimmicks/AltCharacterIdentity.cs`, the per-character SlugBase JSON files, the SlugBase
  dependency) has been deleted from the repo and the installed mod — if any of it ever reappears,
  that's a bug worth investigating on its own, not a sign the design changed back.
- **Mod renamed: AltCharacters → Primal (2026-09-12).** Namespace, assembly name, project folder,
  every class/file whose name derived from "AltCharacters"/"AltChar", and the plugin GUID itself
  (`rokas.altcharacters` → `rokas.primal` — a deliberate choice, understood to orphan any
  already-saved config under the old GUID). `OneSpearGimmick.cs`'s `SaveMarkerKey`
  (`"ALTCHAR_ONESPEAR"`) was deliberately left unrenamed — it's embedded inside actual save files,
  and renaming it would silently reset every save's pull count for no benefit.
- **Sessions moved onto the user's real Windows machine (2026-09-15).** Earlier sessions ran in a
  cloud container with no `.NET` toolchain, no git, and no direct access to the game install —
  changes went through a file-transfer bridge and the user built/deployed by hand. That limitation
  is gone; see "Environment" below for the current, real setup. Do not reintroduce
  container-bridge-style workflow assumptions.

## Environment (verify before assuming either way)

This now runs directly on the user's own machine — real git repo, real toolchain, direct
filesystem access to both the source tree and the actual Steam install. Confirmed repeatedly across
sessions, most recently 2026-11-02:

- `C:\Users\rokas\Documents\RWMods\` — this repo. `git status`/`git log` work normally.
- `cd src/Primal && dotnet build` — builds clean against `.NET Framework 4.8`, 0 errors, in ~1-2s.
- Steam install: `C:\Program Files (x86)\Steam\steamapps\common\Rain World\` — the game root.
  `BepInEx\LogOutput.log` (plugin load log + any `Logger.LogError`/`Plugin.Log` calls this mod
  makes), `exceptionLog.txt`/`consoleLog.txt` (Unity's own per-tick exception logs — **check these
  before theorizing** when a bug is "silent", see Gotcha #1).
- Installed mod: `...\RainWorld_Data\StreamingAssets\mods\Primal\` — `modinfo.json` +
  `plugins\Primal.dll`(+`.pdb`). This is what BepInEx actually loads; a rebuild does **not**
  auto-deploy here (see Build & deploy workflow).
- **Live Remix config**, read/written by the actual running game — NOT in the repo:
  `C:\Users\rokas\AppData\LocalLow\Videocult\Rain World\ModConfigs\rokas.primal.txt`. Every
  per-character gimmick/requirement assignment and setting value lives here once it's ever been
  bound (see **Gotcha #9**) — check this file directly, not just the source code, whenever a
  "changed the default and nothing happened" report comes in.
- `scripts/build-deploy-run.sh` does build + copy-to-install + launch in one step
  (`--no-build`/`--no-launch` for a partial run); see Build & deploy workflow below. It currently
  only copies `modinfo.json` + `Primal.dll` — the artificer scene art under
  `src/Primal/scenes/` is deployed by hand and isn't covered by this script (a real gap, not yet
  fixed — see Current status).
- No `.NET` decompiler is installed by default; `ilspycmd` has been present as a `dotnet` global
  tool in at least one past session (`~/.dotnet/tools/ilspycmd.exe`) — check before assuming it's
  there. When no decompiler is available, a hand-rolled pure-Python ECMA-335 metadata/IL reader is
  a viable fallback (parse the PE/CLI headers and `#~`/`#Strings`/`#US` streams directly) — this has
  been built from scratch in at least one earlier session; worth writing fresh rather than hunting
  for a scratch copy that may not have survived.

## Current architecture

- `PrimalSlot.ReservedSlot = 666666` — the first of a range of save-slot numbers this mod reserves,
  one per real vanilla slot: real slot N's Primal counterpart is `ReservedSlot + N`
  (`PrimalSlotFor`/`RealSlotFor` in `PrimalSlot.cs`). Safe because Rain World only ever uses
  `saveSlot` to build a filename string, never as an index.
- `PrimalSlot.IsActive(RainWorld)` — `rainWorld.options.saveSlot >= ReservedSlot`, a **range**
  check (not equality against one fixed slot — see `PrimalSlot.cs`'s own header for the
  multiple-Primal-slots history). This is the gate every gimmick/requirement patch checks.
- A **gimmick** is one Harmony patch class + a stable string ID, registered in
  `GimmickRegistry.All` (currently 16: `Misery`, `Slugpup`, `OneSpear` ["Master Spear"],
  `TimeLimit`, `NoHold`, `Chains`, `NoYeeks`, `NoCicadas`, `NoMauling`, `NoPassages`,
  `ThreatSlowdown`, `ThreatPanic`, `CreatureKillList` ["Kill List"], `RainTimer`, `BlizzardTimer`,
  `PrecycleTimer`). Each one's own `.cs` file has a header comment with the real implementation
  trace — see the file map below. `GimmickAssignments` is the runtime enabled/disabled lookup
  (Remix-bound, falling back to `GimmickDefaults`); `GimmickGate` is the one shared "should this
  gimmick do anything for this player right now" check every patch funnels through.
- A **Requirement** (`Requirement.cs`/`Requirements.cs`/`RequirementRegistry`) is a separate,
  parallel system: a named, persistent, per-character condition that becomes true and stays true
  (e.g. "reached a given ending", "killed every creature type once"). Currently ~56 registered,
  including the Rain World community's own 50-challenge list (curated in
  `CommunityChallengesGimmick.cs`'s `CommunityChallengesRoster`, cross-referenced against
  `COMMUNITY_CHALLENGES.md`) plus a handful of character-specific ones (Food Quest, Quick
  Broadcast, King, Visit Shelter) and general ones (Ending, Unique Kills). Enabled per character
  through the same `GimmickAssignments` store gimmicks use — a requirement ID and a gimmick ID are
  both just strings to it. `RequirementCompletion` persists satisfied (character, requirement)
  pairs and fires `OnSatisfied` for any consumer (Community Challenges' own cycle-end popup is one;
  nothing assumes it's the only one).
- `PrimalOptions.cs` is the Remix config screen: a **permanent character grid** (one button per
  real vanilla character, colored by `GimmickDef.HomeCharacter`/`PlayerGraphics
  .DefaultSlugcatColor` where set) with a **detail area appended below it** that swaps between a
  character's Gimmicks tab, Requirements tab, a gimmick/requirement's settings sub-view, or an
  add-picker — switching characters always overwrites whatever the detail area was showing. A
  **mode gate** (Primal/read-only vs. Custom/editable) sits in front of this, chosen once per
  session from a splash screen built from `modinfo.json`'s own `name`/`version`/`authors`/
  `description` (so the mode-select screen's title/credits/blurb can never drift from
  `modinfo.json`).
- **Shared helpers** every gimmick/requirement leans on instead of reimplementing: `GimmickGate`
  (the player → real character → `IsActive` → `IsEnabled` gate), `PrimalSaveMarkers` (this mod's
  `unrecognizedSaveStrings` marker convention + a cached lookup), `PrimalMenuAccess` (the one
  reflected `KarmaLadderScreen.saveState` handle menu-side patches share), `ThreatReader` (the
  threat-number lookup both Threat gimmicks read, memoized per frame), `PrimalSettings` (mod-wide,
  not per-character, settings — currently just the Tab slot-switch hotkey), and `SurvivorEvents`/
  `PrimalKillTracker`/the various `Survivor*.cs` files (global, gimmick-agnostic tracking
  mechanisms — room transitions, kills, taming, toll state, etc. — that Requirements subscribe to).
- `PrimalSlotOptionsMenuHooks.cs` adds the 4th "PRIMAL SLOT" button to the vanilla Options menu,
  driving the exact same `Menu.OptionsMenu.SetCurrentlySelectedOfSeries`/`GetCurrentlySelectedOfSeries`
  machinery vanilla's own 3 buttons use. `PrimalSlotTabSwitchHook.cs` adds a Tab-to-switch shortcut
  on the character-select screen with its own black-screen-cover + animated slide transition — see
  that file's own header for the full mechanism (it replicates `OptionsMenu`'s own slot-switch
  three-liner directly, since there's no live `OptionsMenu` instance on that screen to call).
- `RainWorldHooks.cs` — a Postfix on `RainWorld.OnModsInit`. Runs `GimmickDefaults.Apply()`,
  `RequirementDefaults.Apply()`, `RequirementRegistry.RegisterAll()`, registers the Remix screen.
  **Do not move any of this earlier** — see Gotcha #2.

### File map (`src/Primal/`)

Each gimmick/requirement file's own header comment is the authoritative source for its mechanism —
this list is for orientation, not a substitute for reading the file.

- `Plugin.cs` — BepInEx entry point; `OnEnable()` only does `_harmony.PatchAll()` (see Gotcha #2).
- `PrimalSlot.cs` — reserved-slot mapping + `IsActive` gate (see Current architecture).
- `Gimmicks/GimmickDef.cs` / `GimmickRegistry.cs` / `GimmickAssignments.cs` / `GimmickGate.cs` /
  `GimmickDefaults.cs` — the gimmick system itself (definition, registry, runtime
  enabled/disabled store, shared gate, default assignments).
- `Gimmicks/Requirement.cs` / `Requirements.cs` / `RequirementDefaults.cs` — the parallel
  Requirements system (base class + registry + completion tracking in `Requirement.cs`; every
  concrete requirement in `Requirements.cs`, the biggest single file in the project at ~4,100
  lines, one class per requirement with its own detection trace in its own doc comment).
  `COMMUNITY_CHALLENGES.md` is the reference list of all 50 community challenges' names and
  original source descriptions, used when matching up `RequirementRegistry` entries to that list.
- `Gimmicks/PrimalOptions.cs` — the Remix config screen (~1,700 lines: character grid, mode gate,
  Gimmicks/Requirements tabs, settings sub-views, add-pickers). See Current architecture above.
- `Gimmicks/PrimalSaveMarkers.cs` / `PrimalMenuAccess.cs` / `ThreatReader.cs` / `PrimalSettings.cs`
  — shared helpers (see Current architecture above).
- `Gimmicks/RainWorldHooks.cs` — mod-startup wiring (see Current architecture above).
- `Gimmicks/PrimalSlotOptionsMenuHooks.cs` / `PrimalSlotTabSwitchHook.cs` — the two ways to reach
  the Primal slot (Options-menu button; Tab on character-select). See Current architecture above.
- `Gimmicks/PrimalCharacterNames.cs` — relabels every character "PRIMAL &lt;NAME&gt;" on the
  character-select "new game" screen while on the Primal slot, with a written description for the
  4 characters with real content (Gourmand/Spearmaster/Sofanthiel/Artificer — gimmick-driven,
  rebuilt live from `GimmickAssignments`) and a static "Primal version not implemented" for the
  other 6 (Survivor/Monk/Hunter/Rivulet/Saint/Watcher). Edit this file's `if`/`else if` chain to
  change a character's Primal name/description.
- `Gimmicks/PrimalUnimplementedPortraits.cs` — blacks out the character-select portrait for those
  same 6 "not implemented" characters on the Primal slot (Postfix on
  `Menu.MenuDepthIllustration.GrafUpdate`, recoloring the specific "Slugcat" illustration layer).
- `Gimmicks/PrimalPortraitOverride.cs` — a Primal-mode-specific character-select portrait override
  (added during the 2026-09-17 UI redesign — see that file's own header for exactly what it swaps
  and when).
- `Gimmicks/MiseryGimmick.cs`, `SlugpupGimmick.cs`, `OneSpearGimmick.cs`, `TimeLimitGimmick.cs`,
  `NoHoldGimmick.cs` (generalized 2026-09-18 from the old single-purpose `NoSpears`, now deleted),
  `ChainsGimmick.cs`, `NoYeeksGimmick.cs`, `NoCicadasGimmick.cs`, `NoMaulingGimmick.cs`,
  `NoPassagesGimmick.cs`, `ThreatSlowdownGimmick.cs`, `ThreatPanicGimmick.cs`,
  `CreatureKillListGimmick.cs` (+ `PrimalArtificerKillList.cs`), `RainTimerGimmick.cs`,
  `BlizzardTimerGimmick.cs`, `PrecycleTimerGimmick.cs` — one file per gimmick (see Current
  architecture's list above and each file's own header comment). `PrimalArtificerScene.cs` reskins
  Artificer's ending-cutscene art for the Primal slot (see `scenes/primal artificer/`).
  `PrimalKillTracker.cs` is the global (not per-gimmick) "which creature types has this character
  ever killed" tracker both `CreatureKillListGimmick` and the Unique Kills requirement read.
- `Gimmicks/Survivor*.cs` (CheckpointRun, CreatureTracking, CreatureVariant, DreamState,
  EchoEncounter, Events, HazardDeath, MoonDelivery, Pinning, PoleMimicPredation, Predation,
  RegionMarkers, SeedCobEaten, ShelterContents, Taming, TollRoster, TollState, VanillaProgress,
  VultureSignal) — the global, gimmick-agnostic tracking mechanisms the Requirements system
  subscribes to (room transitions, eating, death causes, taming, toll outposts, etc.). Each file's
  own header comment explains exactly which vanilla mechanism it reads and why.
- `CHARACTER_DESIGN_STATUS.md` — a self-contained status audit matching a user-supplied
  per-character gimmick+requirement target table against what's actually implemented; the
  authoritative source for "is X character's intended default setup actually done" as of
  2026-09-18 (see that file directly rather than this doc for the current answer on any specific
  row — it was kept up to date through several follow-up rounds the same day it was written).

## Gotchas

Read these before debugging anything that "silently doesn't work," crashes the Remix menu, or
"does nothing" after a change. Each one cost real time to root-cause; the lesson at the end of each
is the part worth remembering even if the specific symptom never recurs exactly.

### Gotcha #1 — direct field/property access can compile fine and still crash at runtime

The compile-time reference assembly (`lib/PUBLIC-Assembly-CSharp.dll`) is a "publicized" copy of
the game's real assembly, with every field/method's accessibility forced to `public`. **The actual
assembly running on the user's machine still enforces the ORIGINAL (non-public) accessibility.**
Code that directly reads/writes a field that's non-public in the real game (e.g.
`PlayerGraphics.player`) compiles cleanly against the publicized reference, but throws
`FieldAccessException` at actual runtime once Harmony JITs the patch against the real assembly.

This caused the long-running "Spearmaster's grab freezes the game" bug: `OneSpearGimmick.cs`'s
`Prefix` on `setSpearProgress` did `__instance.pGraphics.player`, and `PlayerGraphics.player` is
non-public in the real assembly. The exception threw silently every tick near spear-charge
completion, swallowed by the game's per-creature update loop — the character just stopped
responding to input, nothing in BepInEx's log. **Found via Unity's `exceptionLog.txt`**, not by
reading code. Fixed by reading the field through reflection (`AccessTools.Field(...).GetValue(...)`,
cached), which bypasses the JIT-level accessibility check.

**Lesson:** if a character "just stops responding to input" or something "silently doesn't work"
with nothing in BepInEx's log, stage and read `exceptionLog.txt`/`consoleLog.txt` from the game
root FIRST, before theorizing from code review. Audit for this same class of bug wherever this
codebase directly accesses a field/property on a vanilla type via `.` — if it isn't something a
`[HarmonyPatch]` attribute already targets by name (which goes through reflection and is safe),
check real accessibility before trusting the publicized reference. Method calls patched by name
(`nameof(...)` in `[HarmonyPatch]`) and type tokens (`typeof(X)` in `[HarmonyPatch(typeof(X), ...)]`)
are NOT subject to this — only a plain `.field`/`.Method()` access on a vanilla instance is at risk.

**Applies to `reference/GameSource/` too** — it's decompiled from the same publicized assembly, so
its access modifiers prove nothing either (confirmed: it shows `public Player player;` on
`PlayerGraphics`, the exact field this Gotcha proved non-public). Use it to find a *method* to
Harmony-patch by name (unaffected), not to decide whether a *field* is safe to access directly.

**Applies to methods too, not just fields (confirmed via Gotcha #7's own investigation):**
`ConfigHolder.Save()` shows public in the compile-time reference but threw
`MethodAccessException` at runtime every time it was called directly — fixed the same way, via
`AccessTools.Method(...).Invoke(...)`.

### Gotcha #2 — game-registered content isn't safely readable from `Plugin.OnEnable`

`MoreSlugcatsEnums.SlugcatStatsName.Gourmand`/`.Spear` and similar `ExtEnum` values are registered
by the game itself during its own startup — not compile-time constants. Reading `.value` on them
from `Plugin.OnEnable()` (BepInEx's Chainloader stage, before any `RainWorld` instance exists)
throws `NullReferenceException`. This previously happened inside `GimmickDefaults.Apply()`, called
first thing in `OnEnable()` with nothing catching it — **silently disabling every single Harmony
patch in the mod**, including the PRIMAL SLOT button, with no error in BepInEx's log (just a
missing "loaded" line, easy to miss).

**Fix, in place:** `GimmickDefaults.Apply()`/`RequirementDefaults.Apply()`/Remix-screen
registration all run from a Postfix on `RainWorld.OnModsInit` (`RainWorldHooks.cs`) — the standard
point by which all game-registered enums already exist. `Plugin.OnEnable` now does nothing but
`_harmony.PatchAll()`, wrapped in try/catch with `Logger.LogError`.

**Lesson:** anything touching `MoreSlugcatsEnums`, `WatcherEnums`, or similar game-registered
content must not run from `Plugin.OnEnable`. Use `RainWorld.OnModsInit` (or later).

### Gotcha #3 — verifying real accessibility without a live test: cross-check another installed mod

When it isn't possible to just run the game and check, a cheap, reliable substitute: disassemble
another mod already installed on the user's machine and known to work, and grep it for the same
member access. If a real, currently-running mod already does the exact same direct (non-reflection)
access, that member is genuinely accessible on this game version. Used to greenlight
`RainWorldGame.clock`, `Creature.Die()`, and `Creature.get_dead()` by finding them used directly in
**Rain Meadow** and **Pupify** (both confirmed actually installed/running on this machine).

**Lesson:** when a live test isn't available, "does another real, working mod already do this
exact thing directly" is a solid substitute for guessing from the publicized reference assembly's
access flags alone.

### Gotcha #4 — a `Configurable<T>` can only ever be bound to one UIconfig element at a time

Opening the same gimmick's Remix settings sub-view a **second** time crashed the whole
`OptionInterface` with `Menu.Remix.MultiuseConfigurableException: Configurable X is bound to
multiple UIconfigs`. Root cause: a `Configurable<T>` keeps an internal reference to whichever
UIconfig control (`OpCheckBox`, `OpUpdown`, etc.) is currently bound to it, and `OpTab.RemoveItems`
only detaches an element from the tab's display list — it does **not** unbind it from its
`Configurable`. So rebuilding a fresh `OpCheckBox` against the same `Configurable<bool>` a second
time collided with the still-bound first instance.

**Fix, in place:** every `*Settings`/settings-building class lazily builds its `List<UIelement>`
once per character and caches it in a `Dictionary<string, List<UIelement>>`, returning the exact
same instances on every later call instead of constructing fresh ones.

**Lesson:** any `UIelement` wrapping a `Configurable<T>` must be constructed **at most once** per
`Configurable` for the life of the `OptionInterface` instance. A drill-down/paged Remix UI that
shows the same setting more than once must build it once and cache+reuse — never rebuild fresh.

### Gotcha #5 — a stale, un-rebuilt DLL silently masks every source change, and looks exactly like a broken feature

A reported "did nothing at all" bug turned out to be a build artifact predating every relevant
source change — the DLL being tested simply didn't contain the new code yet. Checking build
timestamps (local `bin/Debug/Primal.dll` / `mod/plugins/Primal.dll` vs. the newest relevant `.cs`
file) ruled this in directly rather than re-deriving the feature's logic from scratch. The deeper
root cause that round: `dotnet build` itself had been silently failing for an unknown number of
past sessions (`Gimmicks/AltCharacterIdentity.cs`, leftover dead code from discarded design #2,
called a method that no longer existed) — meaning an unknown number of earlier "confirmed working"
results may have actually been tested against a stale DLL too.

**Lesson:** when a change "does nothing at all" (as opposed to doing the *wrong* thing), a stale
build/deploy is at least as likely a cause as a silent logic bug — check `dotnet build`'s own
output and artifact timestamps/sizes before re-deriving the feature's logic. A successful
`dotnet build` only updates `bin/Debug/` and (via the csproj's `CopyToModFolder` target)
`mod/plugins/Primal.dll` — it does **not** touch the actually-installed copy at
`...\StreamingAssets\mods\Primal\plugins\Primal.dll`, which still needs a manual (or
scripted) copy every time. A "rebuilt but forgot to redeploy" report looks identical to a real bug.

### Gotcha #6 — leftover Harmony patches from a discarded design can be broken by construction, with no external trigger needed

Deleting a discarded design's *source files* isn't enough if a `[HarmonyPatch]`-decorated class
from that design is still being compiled — it runs every frame regardless of whether anything else
still references it, and can fail in a very active, very visible way even though the design itself
is abandoned. A leftover `SlugcatSelectMenuHooks.cs` (from discarded design #2) was still patching
`SlugcatSelectMenu.Update` every frame; its own "is this character visible" check could never
return true once the registry it depended on stopped being populated, so it burned its per-frame
budget in an infinite loop with no valid target — producing an almost-blank, slow-loading character
select screen that looked like a UI/sizing bug but was actually this.

**Lesson:** when cleaning up a discarded design, find and remove *every* `[HarmonyPatch]` class it
left behind, not just whatever causes a compile error. "Dead code, doesn't run anymore" isn't true
for an active Harmony patch just because the design around it was abandoned.

### Gotcha #7 — Remix's own config-load pass silently skips any OptionInterface with nothing Bound yet

Per-character gimmick/requirement assignments weren't surviving a game restart. Root cause, from
disassembling the real game assembly's Remix/config plumbing:

- `RainWorld.PostModsInit` (runs automatically on every launch) calls `MachineConnector
  ._LoadAllConfigs()`, which calls `_LoadConfigFile()` on every registered `OptionInterface`.
- `OptionInterface._LoadConfigFile()` is a **no-op unless `HasConfigurables()` is already true** —
  i.e. unless `config.Bind()` has already run for at least one key on that OI.
- A newly-registered OI's `Initialize()` (where this mod used to do all its `config.Bind()` calls)
  is only ever called once the player actually opens that mod's own Remix page — nothing calls it
  automatically at startup.
- So on every launch, by the time `PostModsInit`'s automatic load pass ran, `HasConfigurables()`
  was still false, the saved file was never read into memory, and every gimmick's
  `GimmickAssignments.IsEnabled()` fell back to the hardcoded default — looking exactly like
  "assignments silently reset to default every restart."

**Fix, round 1 (load side):** split the config-binding loop into an idempotent
`EnsureConfigBound()`, called eagerly from `RainWorldHooks.Postfix` (during `OnModsInit`, before
`PostModsInit`) in addition to its existing lazy call from `Initialize()`.

**Round 2 (save side) — still didn't work.** Remix's own per-mod config screen has a separate,
native "SAVE" button, distinct from just backing out — which this mod's own full-screen custom UI
gives no indication of and may crowd out entirely. Fixed by having the one method every assignment
change already funnels through call `config.Save()` immediately and unconditionally.

**Round 3 — still didn't work, and this is the one that actually closed the loop.** Checked real
evidence instead of re-theorizing: the user's actual `ModConfigs` folder had **zero bytes ever
written** for this mod, and `LogOutput.log` had the exact answer already logged by round 2's own
try/catch — `ConfigHolder.Save()` was throwing `MethodAccessException` on every call (Gotcha #1,
applying to a method this time, not a field; its `public` flag in the compile-time reference proves
nothing). **Fixed** by calling it through `AccessTools.Method(...).Invoke(...)` instead of
directly — now actually reaches `File.WriteAllText` inside the real `ConfigHolder.Save()`.

**Lessons:**
- A `Configurable<T>` only reflects a previously-saved value once *both* `Bind()` and
  `ConfigHolder.Reload()` have run for that key — any value that needs to be correct on the very
  first frame needs its `Bind()` to happen at `OnModsInit`, not lazily inside `Initialize()`.
- Don't assume a framework's standard UI affordance (a native "SAVE" button) is reachable once a
  mod has replaced that screen's content with its own layout.
- When a defensive try/catch exists specifically to survive an unknown failure mode, the exception
  it logs is the fastest way to find that failure mode — check the log before re-theorizing.
- Checking the actual on-disk artifact a fix is supposed to produce (here: the mod's own
  `ModConfigs\*.txt`) is a strong, cheap sanity check, immediately distinguishing "never wrote
  anything" from subtler bugs like "wrote it to the wrong place."

### Gotcha #8 — an `OpScrollBox`'s own `AddItems`'d children are never cleaned up by the ordinary teardown

A real, reproducible bug found by actually running the game and clicking through the Remix menu
live (not reasoning from disassembly alone): after visiting the add-gimmick picker or any gimmick's
settings sub-view (both wrapped in `OpScrollBox`) and then navigating elsewhere, the new view's
elements sometimes didn't respond to clicks, or a click silently fired a stale, invisible control
left over from the *previous* screen (confirmed directly: a click aimed at one character's grid
button instead fired a leftover row from an earlier picker visit, silently assigning a gimmick to
the wrong character).

**Root cause, from reading the decompiled framework source:** `OpScrollBox.AddItems(items)` adds
each item directly to the owning `OpTab` as its own child — but `PrimalOptions.ShowDetail(items)`
only ever tracked the scroll box *container* in its teardown list, never the rows/controls added to
it afterward (they didn't exist yet when `ShowDetail` was called — the scroll box has to be
attached to the tab before it can accept items at all, see the `OpScrollBox`-ordering note just
below). `OpTab._RemoveItem` detaches an element from the tab's list but **never calls
`Deactivate()`** — the method that cascades into a scroll box's own children — so removing the
by-then-empty-as-far-as-tracking-knows scroll box left every row/control it ever held permanently
attached, still drawn, still wired to its original closure.

**Fix, in place:** both picker and settings renderers now also append the scroll box's
dynamically-built children into the same teardown list, right after handing them to
`scrollBox.AddItems(...)` — no new API, just extending existing bookkeeping to cover these too.

**Separate, related fact about `OpScrollBox` (not itself a leak, but a hard requirement found the
same way):** `OpScrollBox.AddItems` throws `InvalidOperationException` if called before the scroll
box itself has been committed to its `OpTab` — so any renderer using one must call `ShowDetail` (or
equivalent) with the still-empty scroll box first, and only populate its rows afterward in a second
step.

**Lesson, methodology:** this was found by *reproducing the actual behavior live* and reading the
exact framework source that explains it — not by re-guessing pixel coordinates. For any future
"layout looks wrong"/"clicks misbehave" report: get it on screen first; if something behaves oddly,
check whether it's a *stale/leftover element* problem before assuming it's a `pos`/`size` number
that needs nudging — anything going through `OpScrollBox`, or any other framework object with its
own child-tracking, is a candidate. Also: synthetic/automated clicks against this game window have
been unreliable in testing (hover state updates correctly, but `OnClick` itself is intermittent) —
a real in-person test is still the reliable way to confirm any Remix UI fix.

### Gotcha #9 — changing a `Configurable<T>`'s default in source code never retroactively changes an already-saved value

`owner.config.Bind(key, defaultValue, ...)` only ever writes `defaultValue` into
`rokas.primal.txt` the FIRST time that key is ever bound. Every later `Bind` call for that same
key reads whatever's already on disk and ignores `defaultValue` — forever, no matter how many times
the source code's own default changes afterward.

**Why this matters, concretely:** Chains' escape-timer default was fixed from 90 to 0 in code,
rebuilt, redeployed, verified byte-identical on disk — and changed *nothing* observable in-game,
because every character's `*_Chains_escapeTimerSeconds` key had already been written as `90`
during earlier testing, before the fix existed. The fix was correct; it just wasn't reachable,
because the live value was coming from disk, not the constant that got changed.

**General lesson — this will happen again for ANY setting, not just this one.** Once a character
has ever had Remix open (or this mod has ever run with that character selected, which binds every
setting for them regardless of whether a Settings sub-view was actually opened), every one of that
character's `Configurable<T>` keys is permanently written to `rokas.primal.txt` at whatever value
was in effect then. Tuning a default afterward only affects a *fresh* character — fixing a value
for a character/save that already exists requires directly editing (or clearing) their key in the
live config file too. There's no migration mechanism in this codebase for forcing an existing saved
value to a new default; building one (e.g. a version-stamped marker) would be a deliberate choice,
not something that happens for free.

**How this is actually caught:** log the LIVE bound value at the exact moment it's used, not just
that something happened (`UpdateLock`'s escape-timer check now logs the real seconds value) — then
grep `rokas.primal.txt` directly once the log shows a value the source no longer defaults to. Reach
for this on any future "I changed the default and nothing happened" report, before re-suspecting
the surrounding logic.

## Current status (as of 2026-11-02)

**Confirmed working in-game, with real test evidence:**
- **Chains gimmick** — core lock/unlock mechanic, the creature-arrival fix, the shortcut-type
  sealing rules (room-to-room pipes + creature dens locked; within-room shortcuts + creature
  transport left open), and the escape-timer fix are all confirmed via direct play (most recently
  2026-11-02: three separate LC_SUBWAY02 locks, up to 62 creatures tracked, every one cleared
  legitimately via creatures dying — see `ChainsGimmick.cs`'s own header for the full trace).
  Settings-import (banlist checkboxes/presets, catch-late-arrivals, faux-hunted burden) is built
  and present but its own in-game status beyond the core mechanic is unclear.
- **Primal Slot Tab-switch** — base switch + the animated slide transition (rounds 1-3) confirmed
  working. Character preservation across the switch and per-real-slot Primal counterparts (rounds
  4-5) are implemented but not yet confirmed in-game.
- **Config menu UI** — the permanent character-grid-plus-detail-panel layout, settings sub-views,
  and scrollable add-pickers are confirmed working in-game (2026-09-13).

**Implemented, present in current source, builds clean — but never confirmed in-game.** This is
everything else: Misery, Slugpup (including its 2026-09-15 rewrite onto `isSlugpup` rather than
`SlugCatClass`, and its "ends on beating the game" feature), OneSpear/Master Spear, TimeLimit,
NoHold, NoYeeks, NoCicadas, NoMauling, NoPassages, Threat Slowdown, Threat Panic, Kill List, Rain
Timer, Blizzard Timer (including its render-bug fix), Precycle Timer, the entire Requirements
system (all ~56 requirements, including all 50 community challenges), and every UI redesign round
since the character/requirements split (icons, Primal/Custom mode gate, scrollbar/padding fixes).
**Why:** most of this was written during a period with no local build toolchain available, so it
was implemented from careful reading of decompiled vanilla source and cross-referenced reference
mods, but genuinely never compiled or run before being handed off — and the "no toolchain" caveat
attached to each of these in this doc's older history is itself now obsolete (a real toolchain has
been available on this machine since 2026-09-15), which is exactly why **the absence of a build
excuse doesn't mean these are untested — they're specifically waiting on an in-game test pass,**
not a compile check. Each file's own header comment states what to check when testing it. Treat
none of this as "probably fine" until actually played.

**Known issues from real testing, status checked against current source (2026-11-02):**
- **"Gourmand's Misery gimmick also spawns yeeks on room entrance; only the exhaustion effect
  should be part of it."** Checked `MiseryGimmick.cs` directly: it contains no yeek-related code at
  all — only the two documented effects (Jump/WallJump exhaustion-max, PainJumps). Either this was
  already fixed in a session not reflected in this doc, or the report doesn't match current source
  for some other reason. **Unresolved/unclear — re-verify in-game before assuming either way.**
- **"Gourmand does not grow up after visit to OE ending."** `SlugpupGimmick.cs`'s
  `SlugpupEndingHook` (Postfix on `RainWorldGame.BeatGameMode`, checking `!standardVoidSea`) looks
  correctly implemented on inspection and should cover this case per its own header's trace — but
  it has never been confirmed working in-game by anyone (see "implemented, never confirmed" above),
  and this report may be the first real test surfacing an actual bug in it. **Needs investigation**
  — don't assume the existing implementation is correct just because it reads correctly.
- **"New Gimmick — Food. Sets how much food the character needs to eat — one value sets minimum,
  other maximum. Give to Gourmand, default min 7, max 11."** Not implemented — no `Food`-related
  gimmick file exists. **Open feature request.**
- **"Spearmaster's death timer (TimeLimit), set to per-run by default, acted cycle-based instead —
  died after a minute in a shelter, and respawning didn't instantly re-kill, had to wait another
  minute."** `TimeLimitGimmick.cs` does correctly distinguish `RainWorldGame.clock` (per-run) from
  `World.rainCycle.timer` (per-cycle) in its own code — but the reported behavior (surviving a
  shelter sleep, then the timer effectively restarting after respawn) suggests either `game.clock`
  doesn't behave as documented across a sleep/respawn boundary, or something else resets the
  tracked ConditionalWeakTable guard. **Unresolved — needs investigation**, not yet root-caused.
- **"Saint doesn't need the inert-until-cycle-1 gimmick anymore — default to 0, but keep the
  functionality, since it works."** **Resolved.** `NoHoldGimmick.cs`'s generic "inert until cycle
  N" setting already defaults to 0 for every character including Saint (confirmed in
  `CHARACTER_DESIGN_STATUS.md` and `NoHoldSettings`) — no nonzero default was ever specifically
  given to Saint. This also resolves this file's own long-standing "ask the user about this" note
  from before the reorganization.
- **"No Yeeks and No Squidcadas are now superseded by No Hold."** An observation, not yet acted on
  — `NoHoldGimmick.cs`'s Yeeks/Squidcadas categories do cover the same ground as the standalone
  `NoYeeksGimmick`/`NoCicadasGimmick`. Worth considering removing the two standalone gimmicks (and
  their `GimmickRegistry`/`GimmickDefaults` entries) in favor of `NoHold` alone, but **not yet
  done** — ask before removing, since some character's existing default assignment may rely on the
  standalone ID specifically.
- **"Chains shouldn't have visuals coming from creature dens — only from slugcat-enterable pipes
  and shortcuts."** **Resolved in current code.** `ChainsGimmick.cs`'s `ChainState.PipeShortcuts`
  (used for the visual chain anchors) already excludes `CreatureHole` and only includes
  `RoomExit`/`RegionTransportation` — per that field's own header comment, fixed exactly in
  response to this report. Not yet re-confirmed in-game since the fix, same as everything else in
  Chains beyond what's listed as confirmed above.
- **Rivulet's shelter-visit requirement ("Rivulet shelter permanent failure")** — flagged by the
  user as an important issue to eventually fix, but explicitly deferred (see the note at the very
  top of this file). `VisitShelterRequirement` (`Requirements.cs`) is the likely area — its own doc
  comment flags an unverified assumption (whether `ANCIENTSHELTER`/`SU_S05` should really count as
  "visitable") as the most probable source, but this hasn't been investigated yet. **Do not start
  on this unprompted.**

**Known gaps, not bugs:**
- `scripts/build-deploy-run.sh` doesn't deploy the Artificer scene art under `src/Primal/scenes/`
  (only `modinfo.json` + `Primal.dll`) — that art has to be copied to the install by hand. Worth
  fixing the script if this becomes a recurring friction point.
- No gimmick/requirement has been designed yet for Rivulet, Saint, Watcher, Survivor, Monk, or
  Hunter beyond what `CHARACTER_DESIGN_STATUS.md` already covers — Remix checkboxes exist for all
  of them, just no content assigned by default for most.
- TimeLimit has no game-over presentation yet (just an instant kill) — deliberately deferred by the
  user ("later on we can implement some sort of game over screen"); don't build it unprompted.
- Several `Requirement`s in the 50-challenge community list haven't been default-assigned to
  Survivor (or any character) — each exists and is individually assignable via Remix, but there's
  no "give Survivor all 50" default yet; not yet requested.

## Build & deploy workflow

```
cd src/Primal
dotnet build
```

Builds clean (net48) against `lib/*.dll` in ~1-2 seconds; 0 errors is the normal, expected outcome
on this machine. Output: `bin/Debug/Primal.dll`(+`.pdb`) — the project's own `CopyToModFolder`
target also mirrors the DLL into `mod/plugins/Primal.dll` automatically after every build.

**A successful build does NOT deploy to the actual game.** The installed copy BepInEx loads is a
separate file:
```
...\Rain World\RainWorld_Data\StreamingAssets\mods\Primal\plugins\Primal.dll
```
Either run `./scripts/build-deploy-run.sh` (build + copy `modinfo.json`+`Primal.dll` + launch, in
one step — override `PRIMAL_SRC_DIR`/`RAINWORLD_GAME_DIR`/`RAINWORLD_MOD_DEST` env vars if those
paths ever drift), or copy `mod/plugins/Primal.dll`(+ the `bin/Debug/Primal.pdb` for symbol-matched
stack traces) to that path by hand. The script does not currently copy `src/Primal/scenes/` — see
"Known gaps" above.

**If diagnosing a "silently doesn't work" report, in order:**
1. `BepInEx\LogOutput.log` — does the mod show a "loaded" line? Any `LogError`? Any `[GimmickName]`
   log lines showing the actual values/flow (several gimmicks, notably Chains, log fairly verbosely
   — grep for the gimmick's own tag).
2. `exceptionLog.txt`/`consoleLog.txt` at the game root — per-tick exceptions the game's own update
   loop swallows without crashing, invisible to BepInEx's log (see Gotcha #1).
3. `AppData\LocalLow\Videocult\Rain World\ModConfigs\rokas.primal.txt` — the live, actual bound
   value for any setting, which may not match what the source code's own default says (see
   **Gotcha #9**) — check this directly rather than assuming the code's default is what's live.
4. Only then, re-read the mod's own code looking for a logic bug. Every major "invisible" bug found
   in this project so far (Gotchas #1, #2, #7, #9) was found by checking real evidence first, not
   by code review.

## About this reorganization (2026-11-02)

This file used to be ~8,050 lines, almost entirely a chronological day-by-day development diary —
every gimmick/feature's full reverse-engineering trace, every failed attempt and fix-round,
reproduced here a second (often third) time on top of the same material already living in each
`.cs` file's own header comment and, for the 50-challenge system, `COMMUNITY_CHALLENGES.md`. Cross-
checked a representative sample of that duplication directly (Chains, character names, Slugpup,
NoYeeks/NoCicadas/NoMauling, NoPassages, Misery, TimeLimit/NoSpears) and confirmed each `.cs` file's
own header comment already carries the detailed trace this document was repeating.

**What changed:** rewrote the whole file around five things worth keeping centrally — the
architecture overview, the Gotchas (verified against the code, lightly trimmed of obsolete
cloud-container/file-bridge procedural detail that no longer applies, lessons kept intact), History
(the two discarded SlugBase designs + the rename + the move off the cloud container), a single
accurate Current status section built from actually-checkable evidence (what's genuinely confirmed
in-game vs. merely compiles, cross-referenced against the live BepInEx log and the actual
`rokas.primal.txt` config file where possible) and the seven items from the "human written issues"
list that used to sit as an unintegrated appendix at the very end — and an accurate Build & deploy
workflow for the real environment this now runs in (the old one still claimed "there is no git repo
here" and described a cloud-container file-transfer-bridge workflow that stopped being true
2026-09-15). Removed the "Container environment notes" section entirely — every claim in it (no
`dotnet`, no decompiler, proxy-blocked NuGet/package installs) describes a setup this project
hasn't run in since 2026-09-15 and isn't expected to run in again.

**What this did NOT do:** verify every historical implementation claim against a fresh
disassembly (e.g. re-confirming every vanilla IL trace in `Requirements.cs`'s ~56 requirement
classes). That would mean re-doing the original research from scratch, not a documentation
cleanup — the claims are left as originally recorded, on the strength of how thoroughly
cross-referenced they already were against real metadata/IL and other installed mods at the time.
What WAS verified directly: the file/class/gimmick/requirement counts and names in "Current
architecture" (against `GimmickRegistry.All`, `RequirementRegistry.All`, and the actual file
listing), that `dotnet build` succeeds cleanly on this machine right now, that the old "no toolchain"
container caveat is genuinely obsolete, and each "known issue from testing" claim against the
current state of the relevant file.

**Where the old detail went:** nowhere special — it's in this file's own git history if a future
session needs the exact day-by-day account of how a specific already-working mechanism was
originally found (e.g. the precise vertical-slide-transition disassembly trace, or the full
50-challenge-by-50-challenge construction order). It is very unlikely to be needed: every mechanism
that history described is either already captured in its own `.cs` file's header comment, or
superseded by a later round also described there.
