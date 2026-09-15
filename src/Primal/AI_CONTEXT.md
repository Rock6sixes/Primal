# Primal (Rain World mod) — context for picking this back up

Read this whole file before touching code. It exists because this project has been through two
major architecture pivots and several misdiagnosed bugs, and re-deriving that history from the
code alone wastes time. Written 2026-09-11.

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

- `PrimalSlot.ReservedSlot = 666666` — the one save slot number this mod treats specially. Chosen
  because Rain World only ever uses `saveSlot` to build a filename string, never as an index, so an
  arbitrarily large number is safe, and nothing else will plausibly generate it.
- `PrimalSlot.IsActive(RainWorld)` — `rainWorld.options.saveSlot == ReservedSlot`. This is the single
  gate every gimmick patch checks before doing anything.
- A gimmick is: one Harmony patch class + a stable string ID. It reads
  `player.playerState.slugcatCharacter.value` (the player's **real, unfaked** vanilla identity) and
  `GimmickAssignments.IsEnabled(characterID, gimmickID)`, and only then modifies behavior.
- `GimmickRegistry` lists the known gimmicks (currently: `Misery`, `Slugpup`, `OneSpear` (shown in
  the UI as "Master Spear"), `TimeLimit`, `NoSpears`, `Chains`).
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
  below for the pattern; `Misery` and `Slugpup` currently have none (both null).
- `PrimalSlotOptionsMenuHooks.cs` adds the 4th button to the vanilla Options menu and makes it
  drive the exact same `Menu.OptionsMenu.SetCurrentlySelectedOfSeries("SaveSlot", ...)` /
  `GetCurrentlySelectedOfSeries` / `ProgressionBusy` / `HandleSaveSlotChangeFailed` machinery
  vanilla's own 3 save-slot buttons use, via two small Prefixes that translate our virtual button
  index (3) to/from the real reserved slot number (666666). It deliberately does not reimplement
  any slot-switching logic itself.
- `RainWorldHooks.cs` — a Postfix on `RainWorld.OnModsInit`. This is where `GimmickDefaults.Apply()`
  runs and where the Remix options screen gets registered (`MachineConnector.SetRegisteredOI`).
  **Do not move this earlier** — see Gotcha #2.
- `PrimalCharacterNames.cs` (new, 2026-09-11) — while on the alt slot, relabels Gourmand and
  Spearmaster on the "choose your character" screen as "PRIMAL GOURMAND" / "PRIMAL SPEARMASTER",
  with a description listing whichever gimmicks are currently assigned to them. See "Primal
  character names, in detail" below.

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
- `Gimmicks/GimmickRegistry.cs` — the list of all gimmicks (`GimmickRegistry.All`).
- `Gimmicks/GimmickAssignments.cs` — runtime "is gimmick G enabled on character C" lookup
  (Remix-bound `Configurable<bool>`, falling back to a hardcoded default). Unchanged by the UI
  overhaul — `PrimalOptions` still calls `Register` for every (character, gimmick) pair
  exactly as before; only how those pairs are *displayed* changed.
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
- `Gimmicks/SlugpupGimmick.cs` — three patches making up the Slugpup gimmick. `SlugpupGimmick`
  flips `Player.SlugCatClass` to `SlugcatStatsName.Slugpup` right after `GetInitialSlugcatClass`
  sets it (the same trick vanilla's own Jolly Co-op pup-mode uses) — this part alone is confirmed
  working (Gourmand does become pup-sized/pup-shaped and behaves like a pup mechanically).
  `SlugpupStatsGimmick` (Postfix on `Player.NPCStats`'s constructor) and `SlugpupColorGimmick`
  (Postfix on `Player.ShortCutColor()`) are two later additions fixing real bugs the flip alone
  left behind — the pup's physical stats and rendered color were both generic/randomized instead
  of proportional to the real character. See **Slugpup gimmick stats rescaling, in detail** below
  before touching any of this — it documents a genuine multi-round debugging struggle (four
  attempts, three different failure modes) and is **not yet confirmed working as of this doc**.
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
- `Gimmicks/NoSpearsGimmick.cs` — makes spears (`obj is Spear`) ungrabbable for the assigned
  character, by Postfixing `Player.Grabability(PhysicalObject)` (a per-player method, so it can
  single out one character) and forcing the result to `Player.ObjectGrabability.CantGrab` — unless
  that spear's specific type is individually re-allowed in `NoSpearsSettings` (same file: three
  independent toggles, plain/explosive/electric, all default to "not allowed" — matching the
  gimmick's original blanket behavior). Does not interfere with `OneSpearGimmick`'s tail-pull,
  which attaches its spear via `Player.SlugcatGrab(...)` — a different, lower-level method that
  never calls `Grabability`. **Untested by the user as of this doc.**
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
- Requires SlugBase (`slime-cubed.slugbase`) installed as a dependency, even though this mod no
  longer creates any SlugBase characters — it's declared via `[BepInDependency]` and
  `modinfo.json`'s `requirements` purely to control BepInEx's plugin load order (see Gotcha #2).

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
- **Misery, Slugpup**: no settings (`BindSettings`/`BuildSettingsUI` both left null in
  `GimmickRegistry.cs`) — nothing obvious to expose yet. If asked for ideas later: Misery could
  expose the exhaustion multiplier or which of the two effects (instant-max-exhaustion vs.
  forced-PainJumps) are active; Slugpup doesn't have an obvious knob beyond an on/off that already
  exists as the enabled flag itself.

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
Gourmand or Spearmaster, overwrites both with the Primal name and a gimmick-driven description; any
other character (or either of these two off the alt slot) is left completely untouched.

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
always reflects live Remix state rather than a hardcoded snapshot of today's defaults. If a gimmick
is ever added for a character other than the ones already special-cased here, this patch's `else`
branch deliberately leaves that character alone — extend the `if`/`else if` chain rather than
assuming every character should get a "Primal" name.

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
Artificer has been changed by something primal..."). Not yet confirmed in-game as of this doc — see
Outstanding items.

**Build/deploy status:** this feature has gone through several successful builds since it was first
written (the Chains-gimmick and settings-import passes both compiled `PrimalCharacterNames.cs`
along with everything else without incident), so the toolchain/compile-time risk flagged when this
section was first written no longer applies — what's actually unconfirmed now is purely the
in-game behavior: does `AccessTools.Field` actually find `difficultyLabel`/`infoLabel` at runtime,
and does the screen show the expected text. See Outstanding items for the exact checklist.

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

## History — why the architecture is what it is now (don't re-litigate these)

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

## Outstanding items

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
    confirmation: on the alt slot, Gourmand, Spearmaster, and (as of 2026-09-12) Artificer show
    "PRIMAL GOURMAND"/"PRIMAL SPEARMASTER"/"PRIMAL ARTIFICER" with a description listing their
    currently-assigned gimmicks on the "choose your character" (new-game) screen; every other
    character, and all three of these off the alt slot, are unaffected; and toggling a gimmick in
    Remix changes the wording next time that screen is shown.
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
