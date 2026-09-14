# AltCharacters (Rain World mod) — context for picking this back up

Read this whole file before touching code. It exists because this project has been through two
major architecture pivots and several misdiagnosed bugs, and re-deriving that history from the
code alone wastes time. Written 2026-09-11.

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
"ALT CHAR SLOT", alongside vanilla's 3 normal save slots. Switching to it is a completely normal
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

- `AltSlot.ReservedSlot = 666666` — the one save slot number this mod treats specially. Chosen
  because Rain World only ever uses `saveSlot` to build a filename string, never as an index, so an
  arbitrarily large number is safe, and nothing else will plausibly generate it.
- `AltSlot.IsActive(RainWorld)` — `rainWorld.options.saveSlot == ReservedSlot`. This is the single
  gate every gimmick patch checks before doing anything.
- A gimmick is: one Harmony patch class + a stable string ID. It reads
  `player.playerState.slugcatCharacter.value` (the player's **real, unfaked** vanilla identity) and
  `GimmickAssignments.IsEnabled(characterID, gimmickID)`, and only then modifies behavior.
- `GimmickRegistry` lists the known gimmicks (currently: `Misery`, `Slugpup`, `OneSpear` (shown in
  the UI as "Master Spear"), `TimeLimit`, `NoSpears`, `Chains`).
  `GimmickAssignments` is the runtime enabled/disabled lookup (Remix-configured override, falling
  back to a hardcoded default from `GimmickDefaults`). `AltCharactersOptions` builds the Remix menu
  screen as a **permanent character grid with a detail panel appended below it** — see "Config menu
  UI, in detail" below for the full design; the original flat one-checkbox-per-(character,gimmick)
  grid was replaced (because it stopped scaling once the gimmick count started climbing toward ~15)
  by a drill-down navigation UI, which was itself then replaced by the current merged-panel design
  (2026-09-11, at the user's explicit request — see that section for why).
- `GimmickDef` (see `Gimmicks/GimmickDef.cs`) optionally carries `BindSettings`/`BuildSettingsUI` —
  a gimmick can expose its own settings sub-view (e.g. TimeLimit's minutes + per-cycle/per-run
  toggle) beyond the plain enabled/disabled flag. See that file's own doc comments and the section
  below for the pattern; `Misery` and `Slugpup` currently have none (both null).
- `AltCharSlotOptionsMenuHooks.cs` adds the 4th button to the vanilla Options menu and makes it
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

### File map (`src/AltCharacters/`)

- `Plugin.cs` — BepInEx entry point. `OnEnable()` just does `_harmony.PatchAll()`, wrapped in
  try/catch with logging. Deliberately touches nothing game-content-related (see Gotcha #2).
- `AltSlot.cs` — the reserved-slot constant and the `IsActive` gate, described above.
- `Gimmicks/GimmickDef.cs` — data class: ID + display name + description, plus optional
  `BindSettings`/`BuildSettingsUI` hooks for gimmicks with their own settings sub-view (see below).
- `Gimmicks/GimmickRegistry.cs` — the list of all gimmicks (`GimmickRegistry.All`).
- `Gimmicks/GimmickAssignments.cs` — runtime "is gimmick G enabled on character C" lookup
  (Remix-bound `Configurable<bool>`, falling back to a hardcoded default). Unchanged by the UI
  overhaul — `AltCharactersOptions` still calls `Register` for every (character, gimmick) pair
  exactly as before; only how those pairs are *displayed* changed.
- `Gimmicks/GimmickDefaults.cs` — the mod's out-of-the-box gimmick assignments (currently: Gourmand
  gets Misery + Slugpup; Spearmaster gets OneSpear + TimeLimit + NoSpears; Artificer gets Chains).
  Called from `RainWorldHooks`, not `Plugin`.
- `Gimmicks/AltCharactersOptions.cs` — the Remix config screen. Source was changed (2026-09-11) to
  make the character-select grid permanent with a detail area appended below it (see "Config menu
  UI, in detail" below for the intended design) — **but the user reports it still behaves like the
  pre-change drill-down** (clicking a character still makes the grid disappear; some kind of "back"
  navigation brings it back), so as deployed/tested this file's actual in-game behavior does **not**
  yet match the description below. **Left alone for now at the user's request** — do not resume
  debugging this unless asked; see "Outstanding items".
- `Gimmicks/RainWorldHooks.cs` — runs `GimmickDefaults.Apply()` and registers the Remix screen,
  both on `RainWorld.OnModsInit` (see Gotcha #2 for why not earlier).
- `Gimmicks/AltCharSlotOptionsMenuHooks.cs` — the 4th "ALT CHAR SLOT" Options-menu button.
- `Gimmicks/PrimalCharacterNames.cs` — a Postfix on `Menu.SlugcatSelectMenu.SlugcatPageNewGame`'s
  constructor (the "choose your character, read the blurb" screen shown before a run exists on
  that character in that save slot) that relabels Gourmand/Spearmaster as "PRIMAL GOURMAND"/"PRIMAL
  SPEARMASTER" with a gimmick-driven description, but only while `AltSlot.IsActive` — every other
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

### Deployment layout (all on the user's Windows machine, not this container)

- Source: `C:\Users\rokas\Documents\RWMods\src\AltCharacters\` (this file's sibling location).
- Reference DLLs used to compile against: `C:\Users\rokas\Documents\RWMods\lib\`.
- Installed mod: `C:\Program Files (x86)\Steam\steamapps\common\Rain World\RainWorld_Data\
  StreamingAssets\mods\AltCharacters\` — `modinfo.json` + `plugins\AltCharacters.dll`. This is
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
**silently disabling every single Harmony patch in the mod**, including the ALT CHAR SLOT button
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
| `src/AltCharacters/bin/Debug/AltCharacters.dll` (local build output) | **2026-09-10 18:18** |
| `src/AltCharacters/mod/plugins/AltCharacters.dll` (csproj's own post-build copy) | **2026-09-11 04:47** |
| `AltSlot.cs` through `OneSpearGimmick.cs` (that day's earlier gimmick/UI fixes) | 2026-09-11 10:03 – 12:30 |
| `Gimmicks/AltCharactersOptions.cs` (the character-grid/detail-panel UI merge) | 2026-09-11 12:44 |
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
bug:** compare `src/AltCharacters/mod/plugins/AltCharacters.dll`'s `mtimeMs` (via
`device_list_dir`) against the newest `.cs` file's `mtimeMs` in the same tree. If the DLL is older
than source files relevant to the reported bug, the fix is "rebuild and redeploy" before any code
changes, not a debugging session. This only checks the **local** copy inside the source tree — the
actually-loaded copy is a separate file at `C:\Program Files (x86)\Steam\steamapps\common\Rain
World\RainWorld_Data\StreamingAssets\mods\AltCharacters\plugins\AltCharacters.dll`, outside the
`RWMods` connected folder, that can be even further out of date than the local one if the user
forgot the last manual copy step too. **Update (2026-09-11, Gotcha #6's retest):** this path is
reachable this session via `device_request_folder_access` on the Steam game root (see Gotcha #6) —
once granted, this session compared this file's size/mtime directly against the local
`mod/plugins/AltCharacters.dll` and found them mismatched (proving the user's manual copy step
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
themselves and paste the output. It failed — 4x `CS0117: 'AltSlot' does not contain a definition
for 'SetCurrentAltCharacterID'`, all inside `Gimmicks/AltCharacterIdentity.cs`. That file is dead
code from the discarded design #2 (9 SlugBase alt characters + forced session identity — see
History) that was never deleted when `AltSlot.SetCurrentAltCharacterID` was removed during the
pivot to the current design. **This means the project has not compiled successfully since
whichever earlier session made that removal — every `dotnet build` since then has failed, so the
user has been testing an old DLL for an unknown number of sessions' worth of changes, not just
today's.** Grepping the whole source tree confirmed nothing outside that one file references
`AltCharacterIdentity`/`StoryGameSessionIdentityHook`/`AltCharacterIdentityExitGameHook`, so it's
fully isolated dead code, not something anything else depends on.

**Fix, now in place:** `AltCharacters.csproj` gained `<ItemGroup><Compile Remove=
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
`...\StreamingAssets\mods\AltCharacters\plugins\AltCharacters.dll`. Cross-checked via the source
tree's own post-build copy (`mod\plugins\AltCharacters.dll`, which the csproj also updates on every
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
comment in `AltCharacters.csproj` used `--` (this codebase's normal `//`-comment style throughout
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
`AltCharacters.csproj` only excluded that one file; `Characters\*.cs` (9 old SlugBase clone
characters — `AltGourmand`, `AltSpear`, etc. — plus `AllCharacters.cs`) and
`CharacterSelect\*.cs` (`AltRoster.cs`, `SlugcatSelectMenuHooks.cs`) were still being compiled and
loaded like any other file in the mod.

Reading them turned up a bug that's provably broken **by construction, from source alone, with no
need to check what's on disk in the game's `mods\AltCharacters\slugbase\` folder**:

- `SlugcatSelectMenuHooks.cs` is a live `[HarmonyPatch]` on `Menu.SlugcatSelectMenu.Update` — a
  postfix that runs every single frame, stepping `slugcatPageIndex` in a `while` loop until it lands
  on a page whose character counts as "visible" for the currently-mounted save slot
  (`AltRoster.IsVisible`).
- `AltRoster.IsVisible(name, rainWorld)` is defined as `IsAlt(name) == AltSlot.IsActive(rainWorld)`.
- `AltRoster.IsAlt(name)` can only ever return true for a character ID some code registered via
  `AltRoster.RegisterAlt(...)` — and the only callers of that method are the 9 `Characters\Alt*.cs`
  static constructors. Grepped `RainWorldHooks.cs` (the mod's actual `OnModsInit` setup) and
  confirmed it only calls `GimmickDefaults.Apply()` and registers the Remix screen — nothing
  anywhere still forces those 9 static constructors to run (no `AllCharacters.All` iteration, no
  `RunClassConstructor`, nothing). **So `AltRoster`'s internal registry is permanently empty, and
  `IsAlt()` always returns false.**
- That makes `IsVisible` reduce to `false == AltSlot.IsActive(rainWorld)` — true only while
  `AltSlot.IsActive` is itself false. **On this mod's own reserved alt slot** (`IsActive` true,
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

**First fix attempt silently did not reach the device.** `AltCharacters.csproj` was edited locally
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
under `...\mods\AltCharacters\slugbase\` (see Outstanding items) is still installed or not.

**Why the first fix "did nothing": it never actually landed.** Re-staged
`AltCharacters.csproj` directly from the device after the "same thing" report, before assuming the
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
just trusting the write's return value):** `AltCharacters.csproj` has `<Compile Remove>` entries for
`Gimmicks\AltCharacterIdentity.cs`, `Characters\**\*.cs`, and `CharacterSelect\**\*.cs`. Confirmed
safe by grepping the whole tree for `AltRoster`, `AllCharacters`, and `CharacterSelect` outside
those two folders: the only hits were in `AltCharactersOptions.cs`, and those are a same-named but
unrelated local `CharacterEntry[]` array called `AllCharacters` — not a reference to
`Characters.AllCharacters` or the `CharacterSelect` namespace at all. Nothing in the current design
(real vanilla characters + gimmicks gated on `AltSlot.IsActive`) touches either dead folder.
Verified the edited `.csproj` is well-formed XML with
`python3 -c "import xml.etree.ElementTree as ET; ET.parse(...)"` before redelivering (see Gotcha
#5's XML pitfall — avoided `--` in the comment).

**Confirmed fixed (2026-09-11):** after this session copied the correct DLL directly into the
installed game folder (see below), the user relaunched and confirmed the character select screen
works. This gotcha is closed — the two originally-requested items (UI merge, Primal names) are what
still need checking, now that this unrelated interference is actually gone from the running game.

**Second "nothing changed" report, same session:** the user rebuilt and copied the file again and
reported the identical symptom. Re-checked real evidence rather than re-theorizing: `bin/Debug/
AltCharacters.dll` had genuinely shrunk to 31,744 bytes (from 34,304) with a fresh
`CoreCompileInputs.cache` timestamp matching the build — proof the `.csproj` fix compiled correctly
this time. But the actual **installed** copy at `...\StreamingAssets\mods\AltCharacters\plugins\
AltCharacters.dll` was still 34,304 bytes at the old timestamp — the manual copy step into the game
folder hadn't happened. Re-read `exceptionLog.txt` fresh and it showed the byte-for-byte identical
`MethodAccessException` from `SlugcatSelectMenuHooks.Postfix`, confirming the game was still running
the old DLL. Since this session already had folder access to the Steam game root (granted earlier
this session, see above), copied the fixed `mod/plugins/AltCharacters.dll` there directly with
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
values via `config.Bind(...)` inside `AltCharactersOptions.Initialize()`, which is the completely
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
  own)**: split `AltCharactersOptions.Initialize()`'s config-binding loop out into its own
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
  hook. Pushed `AltCharactersOptions.cs` (30,933 bytes) and verified it landed correctly after one
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
   `rokas.altcharacters.txt` at all**. This is the single most useful fact of the whole
   investigation: it proves `config.Save()` was never succeeding even once, for any assignment,
   ever — not "saving but something else clobbers it," not "saving but to the wrong path," not an
   intermittent failure. Genuinely zero bytes ever written.
2. **Checked BepInEx's own log file directly** (`...\Rain World\BepInEx\LogOutput.log`, staged and
   grepped for "AltCharacters") instead of guessing why. It had the exact answer, logged by round
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
  as a static field on `AltCharactersOptions` (resolved once, not per-call), and changed
  `SetEnabledFor()`'s save call from `config.Save();` to `ConfigSaveMethod?.Invoke(config, null);` —
  `AccessTools.Method(...).Invoke(...)` bypasses the runtime accessibility check the exact same way
  `AccessTools.Field(...).GetValue()/SetValue()` already does for fields, so this should now
  actually reach `File.WriteAllText` inside the real `ConfigHolder.Save()` instead of throwing
  before ever getting there. Also corrected the two doc comments (on `EnsureConfigBound()` and
  `SetEnabledFor()`) that previously asserted saving "already worked" / `Save()` is "a plain public
  method" — both were wrong, now point at this section for the full trace. Pushed
  `AltCharactersOptions.cs` (33,612 bytes) and verified via `device_list_dir` that it landed
  correctly on the very first attempt (no silent-write retry needed this time).
- **Not yet built or tested** as of this doc — next: `dotnet build`, copy the DLL, then the same
  end-to-end test as rounds 1–2: assign/unassign a gimmick (e.g. add "No Passages" to Inv again),
  fully quit and relaunch the game **without opening Remix at all**, and this time also check that
  `...\Rain World\ModConfigs\rokas.altcharacters.txt` actually exists and contains the assignment —
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

All three patches gate on `MiseryEffect.IsActiveFor(player)`, which checks `AltSlot.IsActive` +
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

Both gate the usual way (`AltSlot.IsActive` + `GimmickAssignments.IsEnabled`). Neither has been
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
(`AltSlot.IsActive(rainWorld)` + `GimmickAssignments.IsEnabled(characterID, ID)`), are registered
in `GimmickRegistry.All` (confirmed `AltCharactersOptions.cs` still builds every UI loop from that
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
`watcher`, etc. "mods" live, alongside `AltCharacters` itself).

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
(`bin/Debug/AltCharacters.dll` 48,640 bytes) and they copied it to the installed game folder
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
`AltCharacters` itself) to re-verify the whole signature byte-by-byte afterward, rather than
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
Current design, all inside `AltCharactersOptions`:

- **The character grid is permanent.** Built once in `Initialize()` via `BuildCharacterGrid()` and
  added straight to the `OpTab` (not through the wipeable `_detailItems` list) alongside the title
  and subtitle labels — none of the three are ever removed or rebuilt for the rest of the menu's
  life. Same layout as before: a button per real vanilla character (`OpSimpleButton`, the
  `AllCharacters` array), 5 columns x 2 rows (see "Screen is ~600x600, not 1000+" below for why 5x2
  specifically). **`AllCharacters` (top of `AltCharactersOptions.cs`, just past the `CharacterEntry`
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
`.../StreamingAssets/mods/AltCharacters/plugins/AltCharacters.dll` not actually replaced with a
DLL built from the new source, or a build that picked up the old `.cs` from a different output
path) rather than a logic bug in the new source itself — but this is a guess, not a confirmed root
cause, and the alternative (a real bug in the new `AltCharactersOptions.cs` that happens to produce
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
`ContentLeft`/`ContentWidth` (10 / 580) convention in `AltCharactersOptions.cs`, and changing the
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
`GimmickAssignments.IsEnabled` works or when its backing data exists. `AltCharactersOptions` also
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
  `AltCharactersOptions.Initialize()` (via `gimmick.BindSettings?.Invoke(this, character.ID)`,
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
right after that and, only if `AltSlot.IsActive(menu.manager.rainWorld)` and the character is
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
(`AltSlot.ReservedSlot`) and only this one screen (`SlugcatPageNewGame`) — see **"Config menu UI, in
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
irrelevant to AltCharacters' single-character-per-slot design, and was not ported.)

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
**This fix is AltCharacters' own, not ported from Pupify** — Pupify's own decompiled hooks never
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
   `AltSlot.IsActive(rainWorld)` + the player's real vanilla `playerState.slugcatCharacter` identity
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
dead code from a discarded design, still called a method (`AltSlot.SetCurrentAltCharacterID`) that
no longer exists. The project has likely not compiled successfully since whichever session removed
that method, so an unknown number of past "confirmed working" test results may have actually been
against an old DLL too — treat anything not re-verified after this fix with a little extra
suspicion. Fixed by excluding that one file from the build (`AltCharacters.csproj`); every other
file, including both of today's untested features, compiled clean once it was out of the way.
The user re-ran the build clean and, on retesting, reported the in-game character select screen
loading slowly into an almost-blank/wireframe-only state — not a direct answer to either pending
check-item. Root-caused to a **second, independent** piece of leftover dead code from the same
discarded design #2 (see **Gotcha #6**, new): `CharacterSelect\SlugcatSelectMenuHooks.cs`, still
compiled and still an active Harmony patch on `SlugcatSelectMenu.Update`, is broken by construction
on the alt slot specifically (its "which roster is visible" check can never return true for any
character once nothing populates `AltRoster`'s registry anymore), so it spins its full page-search
loop every frame with no valid target. Fixed the same way as Gotcha #5 — excluded `Characters\**`
and `CharacterSelect\**` from compilation via `AltCharacters.csproj`. The user rebuilt and reported
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
`bin/Debug/AltCharacters.dll` had genuinely shrunk to 31,744 bytes this time (proof the fix compiled
correctly), but the **installed** copy at `...\StreamingAssets\mods\AltCharacters\plugins\
AltCharacters.dll` was still the old 34,304-byte file — the manual copy step into the actual game
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
    mods\AltCharacters\slugbase\` (the copy SlugBase actually loaded into the running game).
  - `C:\Users\rokas\Documents\RWMods\src\AltCharacters\mod\slugbase\` (the source tree's master copy
    of the same 9 JSON files).
  - `C:\Users\rokas\Documents\RWMods\src\AltCharacters\Characters\` (9 old SlugBase character
    classes + `AllCharacters.cs`).
  - `C:\Users\rokas\Documents\RWMods\src\AltCharacters\CharacterSelect\` (old roster-filtering UI
    hooks — `AltRoster.cs`, `SlugcatSelectMenuHooks.cs`).
  - `C:\Users\rokas\Documents\RWMods\src\AltCharacters\Gimmicks\AltCharacterIdentity.cs`.
  All confirmed gone via `device_list_dir` re-checks. Since the folders/file these `<Compile
  Remove>` entries pointed at no longer exist, removed all three exclusion lines and their two long
  explanatory comments from `AltCharacters.csproj` too (harmless to leave pointing at nothing, but
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
  project's post-build step mirrors that into `mod/plugins/AltCharacters.dll`, confirmed in sync,
  42,496 bytes) — it does **not** touch the actually-installed copy the game loads,
  `...\StreamingAssets\mods\AltCharacters\plugins\AltCharacters.dll`, which was still the old,
  pre-Chains 33,792-byte build. Confirmed via `device_list_dir` on both paths before touching
  anything (the installed copy's own timestamp predated `ChainsGimmick.cs` entirely) rather than
  guessing. Since this session already has folder access to the Steam game root (from Gotcha #5's
  original fix), copied the fresh DLL+PDB there directly — first `device_commit_files` attempt
  reported success but the size hadn't actually changed on re-check (the same recurring silent-write
  glitch documented throughout this file's Slugpup section), retried with `force: true`, and this
  time verified the installed copy is genuinely 42,496 bytes, matching the fresh build exactly.
  Also spot-checked (2026-09-12) that `AltCharactersOptions.cs` builds every one of its
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
  **succeeded** (`bin/Debug/AltCharacters.dll` 44,544 bytes, `mod/plugins/` mirror confirmed in
  sync) — same deploy-gap lesson applied proactively this time: copied the fresh DLL+PDB straight to
  the installed game folder myself before reporting anything done
  (`...\StreamingAssets\mods\AltCharacters\plugins\`, previously the pre-arrival-handling
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
  (`bin/Debug/AltCharacters.dll` 48,640 bytes) and they copied it to the installed game folder
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
  `AltCharactersOptions.AllCharacters` as "Inv" (web-searched to confirm "Inv"/"Sofanthiel" really
  is More Slugcats' secret slugcat rather than guessing — see "Config menu UI, in detail" above),
  and default-assigned NoYeeks/NoCicadas/NoMauling/NoSpears to it in `GimmickDefaults.cs`; (2) a new
  "No Passages" gimmick — Passages stay visible on the cycle-complete/death screen but their button
  is disabled via the real vanilla `ButtonBehavior.greyedOut` flag (see "NoPassages gimmick, in
  detail" above for the full disassembly trace), registered with no default assignment. Pushed all
  four touched files (`AltCharactersOptions.cs` 23,087 bytes, `GimmickRegistry.cs` 9,347 bytes,
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
  `AltCharactersOptions.EnsureConfigBound()`, called eagerly from `RainWorldHooks.Postfix` (during
  `OnModsInit`, before `PostModsInit` fires) in addition to its existing lazy call from
  `Initialize()`. **The user tested this and reported it still didn't work** — "dotnet build,
  copied files, added no passages to inv, reset game, no change. The game did not however give me
  an option to apply the saved config of the remix settings." That surfaced a second, more
  fundamental bug round 1 had wrongly assumed away.
  **Round 2** attempted to fix the *save* side: Remix's per-mod config screen has its own dedicated
  native "SAVE" button (separate from just backing out of the screen), which this mod's own
  full-screen character-grid UI gives no indication of and likely crowds out entirely. Fixed (or so
  it seemed) by having `AltCharactersOptions.SetEnabledFor()` (the single choke point both
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
  `AltCharactersOptions.cs` (33,612 bytes, round 3) and verified it landed correctly on the first
  attempt (no retry needed). **Not yet built or tested** — next: `dotnet build`, copy the DLL, then:
  assign/unassign a gimmick, fully quit and relaunch the game *without* opening Remix again, confirm
  the assignment held, **and this time also check `...\Rain World\ModConfigs\rokas.altcharacters.txt`
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
of `AltCharactersOptions.cs` that were kept unchanged, since those were already known to compile)
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
cd src/AltCharacters
dotnet build            # net48, builds clean against lib/*.dll (see .csproj) -- when a toolchain
                         # is actually available in this container; see "Container environment
                         # notes" above if it isn't.
```

Output: `bin/Debug/AltCharacters.dll`. To ship a change to the user's machine (this container has
no direct access to it — everything goes through `SendUserFile` + the remote-devices bridge):

1. Copy the built DLL to `/mnt/user-data/outputs/AltCharacters.dll` (and any changed `.cs` files,
   for keeping the user's own source tree in sync).
2. `SendUserFile` on those paths to get `file_uuid`s.
3. `device_commit_files` each to its real device path:
   - DLL → `C:\Program Files (x86)\Steam\steamapps\common\Rain World\RainWorld_Data\
     StreamingAssets\mods\AltCharacters\plugins\AltCharacters.dll`
   - Each changed `.cs` → its mirrored path under
     `C:\Users\rokas\Documents\RWMods\src\AltCharacters\...`

There is no git repo here (`git` was never initialized in this working tree) — treat the source
tree as the single source of truth on the user's own machine; this container's copy of it is
whatever was staged in for the current session and is not authoritative between sessions.

If diagnosing a "silently doesn't work" report: stage and read, in this order,
`BepInEx\LogOutput.log` (does the mod even show a "loaded" line? any `LogError`?), then
`exceptionLog.txt` / `consoleLog.txt` at the game root (per-tick exceptions the game's own update
loop swallows without crashing, invisible to BepInEx's log) — **before** re-reading this mod's own
code looking for the bug. Both prior major "invisible" bugs in this mod (Gotchas #1 and #2) were
found this way, not by code review.
