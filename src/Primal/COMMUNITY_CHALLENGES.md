# Community Challenges tracker -- feasibility pass (2026-09-16)

Goal (per the user, verbatim): one gimmick -- planned name "Survivor" -- that tracks every
informal Rain World community challenge inside a single run, crediting as many as possible.
This doc is the first pass at that: a per-challenge rating (1 = trivial to track, 5 = hard
research project) of how difficult it would be to detect **that a challenge's condition was
met**, not how hard the challenge is to play. Nothing in this doc has been implemented yet.

**Important scoping decision, made here rather than asked about, because it changes several
ratings below:** several challenges carry a "no exploit/no specific input technique" clause
for the human reviewers judging a submitted video (no wallpounces, batspears-only, no karma
caching, no guardian skip, a specific movement tech required, etc.). A save-tracker has no
reliable way to distinguish *how* the player reached a state from a video review's own frame-
by-frame judgment call, and this codebase's other credit-based gimmicks (Kill List, OneSpear)
already work the same way -- they award credit for an outcome, they aren't an anti-cheat
referee. So the plan below is: **track the state condition the challenge describes (position,
creature identity/color, item counts, region, timing, alive/dead status), and leave every
"and don't use technique X to get there" clause as an honor-system note, the same as a human
reviewer's own clip check.** Ratings assume this scope, not full rules enforcement.

Also worth noting up front: **every one of these challenges is vanilla-only** ("The More
Slugcats Expansion must be disabled for all challenges") -- unlike the rest of this mod's
gimmicks, which are almost all homed on MSC/Watcher characters, this tracker never needs
`ModManager.MSC`/`ModManager.Watcher` gating (see the `PrimalOptions.AllCharacters` bug fixed
a few commits back for why that gating matters elsewhere) and can stick to plain
`CreatureTemplate.Type`/`SlugcatStats.Name`/region-acronym values throughout.

## Rating scale

1. Trivial -- reuses an existing gimmick's mechanism almost as-is, or reads a value vanilla
   already tracks itself.
2. Easy -- one new but simple check (a position/room/region match, a single object type, a
   per-cycle counter), mostly data-entry (finding the right room name/coordinates) rather than
   new mechanism.
3. Moderate -- a genuinely new mechanism, but one with a small number of moving parts and no
   deep-research unknowns (an object-in-shelter count, a two-region carry, a stat threshold).
4. Hard -- needs either a new **generalizable** mechanism this session hasn't built yet
   (cross-region persistent multi-creature tracking, creature color/variant identification,
   toll/reputation state) or several of the moderate mechanisms combined.
5. Hardest -- an obscure, single-purpose vanilla mechanic with no existing trace in this
   codebase, needing real decompilation research before any tracking code can be written at
   all, and/or a condition too ill-defined to formalize without more rules research.

## Per-challenge ratings

| # | Challenge | Rating | What it needs / why |
|---|---|---|---|
| 1 | Flytrap | 4 | Pole plants aren't creatures -- their "grab and slowly kill" mechanic is a hazard object this codebase has never touched. Needs decompiling that object before a live-vulture-eaten event can even be hooked. |
| 2 | Visitor | 2 | One boolean (left tutorial) + a hardcoded room+position zone re-entry check, before the room's own rain/flood state. Data-entry heavy, mechanism is simple. |
| 3 | Chieftain | 4 | "Bring live Scavengers" means tracking specific `AbstractCreature` IDs staying alive across a region transition to Moon -- a persistent multi-creature-across-regions mechanism this codebase doesn't have yet (creatures aren't grabbed items; they follow on their own). |
| 4 | Manslaughter | 5 | Needs toll detection, enumerating every guard (including *replacement* scavengers spawned after the first dies), confirming every one dies with no player kill-credit AND no faction-hostility change simultaneously. Several conditions across several creatures at once. |
| 5 | Tasty | 2 | Same death-cause-attribution shape as the existing Kill List gimmick, plus "pulled into a den" (not a shelter) as the location check. |
| 6 | Leukocyte | 2 | Same shape as Tasty -- specific killer type + "died inside a shelter room" location check. |
| 7 | Heist | 4 | Needs toll payment/reputation state (bypassing payment, then confirming the toll's scavengers actually went hostile) -- an obscure system this mod has never read, plus the pearl-in-shelter-before-rain check. |
| 8 | Fishtank | 5 | 7 *distinct-spawn-den* Hazers, individually tracked alive across however many cycles it takes to gather them, then confirmed simultaneously alive in one flooded room. Multi-creature persistence at its most demanding. |
| 9 | Descension | 2 | Death-cause (fall damage) + region-of-death, added to a persisted per-save set across as many deaths as it takes -- this is exactly the marker-cache shape `PrimalSaveMarkers`/Kill List already use, just keyed on region instead of creature type. |
| 10 | Visions | 2 | Per-cycle counter on a single "ate a Karma Flower" event, reset at cycle start. |
| 11 | Mycologist | 2 | Same per-cycle "ate X" counter as Visions, with a region-diversity set instead of a raw count, and a cycle-0 check. |
| 12 | Catalogue | 3 | New "objects present in this shelter at hibernation, deduplicated by icon" mechanism -- moderate, and partly reuses `KillListDialog`'s existing icon-lookup code. |
| 13 | Despicable | 3 | Needs to read (or hook) whichever field `DreamsState` uses to pick a dream, to identify "Stolen Enlightenment" specifically, plus a cycle-0 check. |
| 14 | Elder | 4 | Same toll/multi-scavenger tracking shape as Manslaughter, plus detecting "is wearing a Vulture Mask" on each one and counting 6 simultaneously alive. |
| 15 | Feast | 3 | Needs a real Red-Centipede color/variant check (unclear yet whether that's its own `CreatureTemplate.Type` like lizard colors, or a runtime color stat -- worth checking before rating this higher or lower), otherwise reuses existing kill+eat attribution. |
| 16 | Desperado | 2 | One hardcoded room, two hardcoded objects, an eaten-both flag, and a room-exit-alive check. Fully self-contained. |
| 17 | Symbiote | 3 | Starving-state check (already a real `Player` field) + carrying a specific creature type across two hardcoded shelters within one cycle -- a first, simpler case of the "carry a live creature between two points" mechanism several other challenges also need. |
| 18 | Vegan | 3 | 7 hardcoded fruit positions in the tutorial + a "haven't crossed this position threshold yet" gate. Per the scoping note above, the actual movement-tech requirement is left to video review, not tracked. |
| 19 | Friend | 3 | Revised down 2026-09-16: the "tamed" mechanism is now confirmed (`ArtificialIntelligence.friendTracker.friend == player`, already used by both `ChainsGimmick.cs` and `SleepoverTracker`) -- what's left is checking it for each of ~9 Lizard species and tracking the result as a **save-file-wide** set rather than per-character, the first challenge here that doesn't fit this mod's existing per-character marker scoping and needs a new save-scope path. |
| 20 | Collector | 2 | Vanilla already tracks Sandbox unlocks in its own progression data -- almost certainly a straight read of an existing field once found, no new tracking logic. |
| 21 | Nomad | 2 | 4 hardcoded karma-gate room names in one region + a "crossed" event per gate + a per-cycle reset. Mostly data-entry. |
| 22 | Enlightenment | 4 | Neurons are a follow-you object, not a grabbed item -- needs its own tracking mechanism, a passage-usage detection hook (obscure), and a "total neuron count," not a simple "7 new" counter, per the challenge's own note. |
| 23 | Journey | 2 | Hardcoded start shelter + destination-region arrival check within one cycle -- same shape as Nomad/Desperado. |
| 24 | Charged | 3 | Same "carry a specific live creature into a specific hardcoded shelter and sleep" shape as Symbiote, single cycle. |
| 25 | Passages | 2 | Delegates entirely to Steam's own achievement state via whatever wrapper the game's own `SteamManager`-equivalent uses -- assuming that's readable at all (needs a quick check), this is close to free. |
| 26 | Scholar | 3 | Same "objects present in shelter, deduplicated" mechanism as Catalogue, keyed on `DataPearl.dataPearlType` (color) instead of icon -- needs the full pearl-color roster enumerated once (data-entry). |
| 27 | Martyr | 4 | Needs the "rot" stat used inside Unfortunate Development (an obscure, single-region hazard stat this mod has never read) plus a specific-room-reached-then-returned-to-start check. |
| 28 | Devotee | 3 | If vanilla already tracks which Echoes have been met (worth checking -- it's plausible, since campaign completion gates on this), this mostly becomes a read of that plus a "started this cycle from the same shelter every time" check. Rated on the assumption that read isn't free. |
| 29 | Offering | 4 | Needs a Lantern Mouse color check (purple/green) and an albino-Leviathan variant check, plus hooking a Leviathan-eats-held-creature interaction this mod has never touched -- a rare, likely under-documented mechanic. |
| 30 | Sleepover | **Implemented** | Built 2026-09-16 -- "not tamed" confirmed via `ArtificialIntelligence.friendTracker.friend`, "not killed" via the existing live-creature-in-shelter check, plus a region check. |
| 31 | Saint | 5 | "Complete all 3 objectives" is under-specified without more rules research, and its exclusions (no karma caching, no echo-negated kills, no guardian skip) are exactly the kind of clause this doc's scoping note says isn't reliably trackable at all -- this one may end up partially honor-system by necessity, not just by choice. |
| 32 | Safari | 3 | Three specific creatures/variants alive in the same room simultaneously -- easy IF Red Lizard and Red Centipede both turn out to be distinct `CreatureTemplate.Type` values (Lizard colors already are); needs that confirmed before finalizing. |
| 33 | Usurper | 4 | Needs a "demasked while still alive" King Vulture interaction (related to, but distinct from, the kill-time mask-drop this mod may already see incidentally) tracked twice in one cycle, plus both masks present at hibernation. |
| 34 | Triplopia | 4 | "Triple-spawn" is a den-respawn-count exploit with no obvious existing flag to read -- detecting it likely means an indirect heuristic (3x the normal creature count from one den in a short window) rather than a clean state check. |
| 35 | Ranger | 1 | Directly reads the existing Kill List gimmick's own per-character kill set -- just a "distinct species killed this run >= 32" threshold on data that's already tracked. |
| 36 | Spearfisher | 2 | Same Kill List extension as Ranger, single creature type -- rated one above Ranger only because Leviathan kills may not flow through the same `Die()`/`killTag` path every other tracked creature does, worth confirming. |
| 37 | Ascension | 3 | This mod already reads `BeatGameMode`'s `standardVoidSea` flag for the Slugpup-ending setting -- reuses that same ending hook, plus reading whatever field the Remix speedrun timer itself already stores rather than timing it independently. |
| 38 | Outlaw | 2 | Hunter's own score is already computed and shown on vanilla's own end-of-run score screen -- almost certainly a direct read of an existing field. |
| 39 | Circuit | 3 | A reusable "checkpoint speedrun segment" mechanism (start shelter, N checkpoint rooms in order, return to start, elapsed IGT under a limit) -- new, but a clean, self-contained pattern worth building once and reusing (see U-Turn). |
| 40 | Mother | 2 | Same "N of one object type present in shelter at hibernation" shape as several others, cycle-0 constraint, single object type. |
| 41 | Chef | 4 | Albino-Jetfish color check + cross-region carry (Subterranean -> Five Pebbles) + a specific "killed on a zapper" cause check -- three moderately obscure pieces stacked. |
| 42 | Foe | 2 | Direct extension of Kill List, scoped to the 9 Lizard-type IDs and reset per-cycle instead of per-save -- the one new piece is a cycle-scoped variant of an already-built mechanism. |
| 43 | Ornaments | 4 | "Pinned to a surface, alive" needs its own state check this mod has never looked for (whatever flag/condition a spear-stuck-to-wall creature actually has), counted per unique species per cycle. |
| 44 | Broadcast | 5 | The Communications Array signal/laser mechanic (power level, color state) is a single-purpose, obscure system with no existing trace in this codebase or its reference decompiles explored so far -- needs dedicated research before any of it can be tracked. |
| 45 | Guardian | 4 | Two creatures (Grapple Worm + Snail) carried simultaneously across a region, a non-starving-survival check across a full cycle, and a final "met the Guardians" event -- combines the multi-creature-carry mechanism with an ending-adjacent trigger. |
| 46 | Massacre | 2 | If the same creature-point-value table Hunter/Outlaw scoring uses is readable/enumerable, this is a straight per-cycle point sum against it; if not, it downgrades to hand-copying that table as static data (still not hard, just tedious). |
| 47 | Pilgrimage | 4 | Cyan-Lizard color check + a live carry across 3 named regions to a specific destination room -- the multi-region persistent-carry mechanism again, at its more demanding end. |
| 48 | Criminal | 3 | Reuses the toll-payment-bypass detection Heist needs, applied 3 times in sequence within one cycle, plus a final room-arrival check. Rated a point below Heist since it doesn't also need reputation/hostility state, just "did or didn't pay." |
| 49 | U-Turn | 2 | Same "loop return within a time limit" shape as Circuit but without named mid-route checkpoints -- simpler instance of that same reusable mechanism. |
| 50 | Perseverance | 3 | Reuses the same void-sea/ascension ending hook as Ascension, plus a cycle-0 start check; "no Guardian skip" is left as an honor-system note per the scoping decision above, same reasoning as Saint's exclusions. |

## Rating distribution

- **1 (trivial):** Ranger.
- **2 (easy):** Visitor, Tasty, Leukocyte, Descension, Visions, Mycologist, Desperado,
  Collector, Nomad, Journey, Passages, Spearfisher, Outlaw, Mother, Foe, Massacre, U-Turn.
  (17)
- **3 (moderate):** Catalogue, Despicable, Feast, Symbiote, Vegan, Charged, Scholar, Devotee,
  Sleepover, Safari, Ascension, Circuit, Criminal, Perseverance. (14)
- **4 (hard):** Flytrap, Chieftain, Heist, Elder, Friend, Enlightenment, Martyr, Offering,
  Usurper, Triplopia, Chef, Ornaments, Guardian, Pilgrimage. (14)
- **5 (hardest):** Manslaughter, Fishtank, Saint, Broadcast. (4)

## Shared mechanisms worth building once, used by many

Rather than building each challenge bespoke, most of the 4/5-rated ones (and several of the
2/3s) fall out of a handful of reusable pieces:

1. **Region-scoped death-cause markers** (rating-2 tier: Descension, Tasty, Leukocyte) --
   directly extends the existing `PrimalSaveMarkers`/Kill List pattern, just keyed on
   region-or-cause instead of creature type. Build first; it's nearly free given what already
   exists.
2. **"N objects/creatures present in this shelter at hibernation, deduplicated by some key"**
   (Catalogue, Scholar, Mother, and the simpler end of Charged/Sleepover/Symbiote) -- one
   generic mechanism parameterized by the dedup key (icon, pearl color, creature type),
   reusing `KillListDialog`'s existing icon-lookup code for the icon case.
3. **Cross-region persistent "carry this creature alive"** (Chieftain, Fishtank, Symbiote,
   Charged, Offering, Chef, Guardian, Pilgrimage) -- the single biggest recurring need and the
   one piece of genuinely new infrastructure worth designing carefully up front, since 8
   challenges depend on some version of it. Worth prototyping on the simplest case (Symbiote:
   one creature, two hardcoded points, one cycle) before tackling Fishtank's 7-creature/
   unique-spawn-den version.
4. **Creature color/variant identification** (Feast, Safari, Sleepover, Offering, Chef,
   Pilgrimage) -- needs one research pass to nail down how each variant (Red Lizard, Red
   Centipede, purple/green Lantern Mouse, albino Leviathan/Jetfish, Cyan Lizard) is actually
   represented (separate `CreatureTemplate.Type` vs. a runtime color/stat field) before any of
   these can be rated more precisely than "3-4."
5. **Toll/reputation/payment state** (Heist, Manslaughter, Elder, Usurper, Criminal) -- a
   second cluster of obscure-but-shared state, worth one research pass covering all five at
   once rather than five separate investigations.
6. **Checkpoint speedrun segment** (Circuit, U-Turn) -- start shelter, elapsed-IGT limit,
   optional ordered checkpoint rooms, return-to-start. Small, self-contained, reusable.
7. **Ending-hook reuse** (Ascension, Perseverance) -- this mod already has the exact
   `BeatGameMode`/`standardVoidSea` hook Slugpup's ending setting uses; both challenges are
   nearly free extensions of it.
8. **Vanilla-tracked state, just needs reading** (Collector's Sandbox unlocks, Outlaw's Hunter
   score, Passages' Steam achievements, possibly Devotee's Echo-met list, possibly Massacre's
   point-value table) -- worth a quick decompile pass across all five before writing any new
   tracking logic, since some of this may already be sitting in a field this mod can just read.

## Genuinely bespoke, no shared payoff

Flytrap (pole plants), Despicable (dream selection), Martyr (rot stat), Triplopia
(triple-spawn heuristic), Broadcast (signal/laser), Ornaments (pin state), Enlightenment
(neuron-follow + passage detection), and Saint (under-specified objectives) each need their
own one-off research and don't obviously feed into anything else on this list. Natural
candidates to save for last, or to descope first if the gimmick ships in stages.

**Nothing in this doc has been built, and none of the "needs confirming" points above have
been checked against the decompiled source yet** -- this is a planning pass only, done before
writing any tracking code, per the user's own request.

## Status update, 2026-09-16: shared mechanisms implemented

All 8 mechanisms above are now built, on the `survivor-gimmick` branch, as pure infrastructure
-- no per-challenge gimmick logic yet, per the user's own request that day. Every API was
checked against the decompiled `reference/GameSource/` before being written, not guessed; see
each file's own doc comment for its specific traces. Still completely unbuilt/untested (no
.NET toolchain in this container).

- `SurvivorEvents.cs` -- the event bus behind mechanism #7 (ending-hook reuse) and the
  detection half of #1 (fall-damage death): Prefix on `RainWorldGame.Win` (before its
  synchronous save write, same lesson as the Kill List Postfix/Prefix bug), Postfix on
  `GameOver` and `BeatGameMode`, and a wasDead/isDead Prefix+Postfix pair on
  `Player.TerrainImpact` that proves a death was specifically the fall-damage branch without
  re-deriving vanilla's own threshold formula (confirmed: exactly one `Die()` call in that
  whole method).
- `SurvivorRegionMarkers.cs` -- mechanism #1's persistence half, a thin wrapper reusing
  `PrimalSaveMarkers` verbatim (needed no new persistence code at all).
- `SurvivorShelterContents.cs` -- mechanism #2, query-only, built on the confirmed
  `AbstractRoom.entities`/`AbstractRoom.shelter` fields.
- `SurvivorCreatureTracking.cs` -- mechanism #3. Found and worked around a real bug in the
  base game's own `EntityID` struct: `Equals` compares only the `number` field, but
  `GetHashCode` falls through to `ValueType.GetHashCode()`, which hashes every field --
  violating the Equals/GetHashCode contract .NET's own hashed collections require. Everything
  here keys on the plain `int number` instead of the struct itself. Also confirmed there's no
  global "find creature by ID" lookup anywhere in the decompiled source, so this tracks
  last-known references rather than a live index -- documented as a real limitation, not
  hidden.
- `SurvivorCreatureVariant.cs` -- mechanism #4, and the single biggest finding of this pass:
  **most of it doesn't need to exist.** Lizard colors (RedLizard, CyanLizard, ...) and
  Centipede's red variant (RedCentipede) are each their OWN `CreatureTemplate.Type` value in
  vanilla, not a runtime color field -- exactly the enum this mod's Kill List already compares
  directly. That drops Feast, Sleepover, Pilgrimage, and two of Safari's three creatures to the
  same "1 -- direct CreatureTemplate.Type check" tier as Ranger/Spearfisher, no longer "4".
  What's left and actually implemented: `BigEel.albino`/`JetFish.albino`, both confirmed real
  `bool` fields, read through `AccessTools` per Gotcha #1. Lantern Mouse's purple/green is a
  genuine continuous HSLColor hue with no natural threshold and is deliberately left
  unimplemented (see the file's own comment on why guessing a hue range is closer to this mod's
  honor-system carve-outs than a clean check).
- `SurvivorTollState.cs` -- mechanism #5. All five toll challenges turned out to share one
  class, `ScavengerOutpost`, confirmed to already expose everything needed: `team` (guard
  roster, self-updating when a replacement scavenger spawns in), `worldOutpost.feePayed > 9`
  (vanilla's own "paid enough" threshold, reused rather than invented), and
  `playerTrackers[...].killOrder` (vanilla's own per-player hostility flag), the last one
  surfaced live via a Postfix on `ScavengerReportTransgression`. Not yet covered: telling
  "stole without paying" apart from "attacked a scavenger" -- both call the same method as far
  as this pass's research went.
- `SurvivorCheckpointRun.cs` -- mechanism #6, a small reusable timer/checkpoint state machine
  against `RainWorldGame.clock` (confirmed 40 ticks/second, same field TimeLimitGimmick already
  relies on) -- deliberately NOT the `MoreSlugcats`-namespaced `SpeedRunTimer` HUD class some of
  these challenges' own rules reference, since that namespace pattern is exactly what broke
  `PrimalOptions.AllCharacters` under MSC-disabled a few commits ago.
- `SurvivorVanillaProgress.cs` -- mechanism #8, confirming (not guessing) four separate
  vanilla-state reads: Sandbox unlocks (`PlayerProgression.miscProgressionData
  .GetTokenCollected`), Steam achievements (`RainWorld.AchievementAlreadyDisplayed`, Steam-only
  for now -- the user's own install is Steam), Echo-met tracking for Devotee
  (`DeathPersistentSaveData.ghostsTalkedTo`, which vanilla itself already writes to on every
  real Echo encounter), and the creature kill-point-value table Massacre/Outlaw/Ranger's own
  rules reference (`Menu.SandboxSettingsInterface.DefaultKillScores`, looked up per
  `CreatureTemplate.Type` via the same ExtEnum string-round-trip `KillListEntry` already uses,
  rather than hand-copying vanilla's own table as a second, driftable copy).

**Ratings this pass revises down**, given the above: Feast, Sleepover, Pilgrimage (now ~1-2:
direct `CreatureTemplate.Type` checks, no variant research needed), Collector, Devotee, Outlaw,
Passages, Massacre (now ~1-2: confirmed direct reads of vanilla's own already-tracked state).
Safari stays slightly higher than those since one of its three creatures (Spitter Spider) wasn't
checked this pass. Every other rating in the table above still stands.

## Status update, 2026-09-16 (later): first 3 trackers implemented

`ChallengeTrackers.cs` now has real `ChallengeTracker` subclasses for **Descension**, **Ranger**,
and **Spearfisher** -- picked without deliberating over which were "most" easy, per explicit
request; all three turned out to need essentially no new detection logic at all:

- **Descension** needed only the 12-region roster itself (confirmed via decompiling
  `Menu/RegionSelectMenu.cs`'s own ordered acronym/display-name pairing: SU, DS, HI, CC, SI, GW,
  SH, SL, LF, UW, SB, SS) -- `SurvivorEvents.OnFallDamageDeath` and `SurvivorRegionMarkers`
  already did all the actual detection and persistence from the shared-mechanisms pass.
- **Ranger** and **Spearfisher** both reduce to a straight read of the Kill List gimmick's own
  already-tracked permanent kill records (`KillListTracker.IsKilled`) -- no detection mechanism
  of their own, just a threshold check (32 unique species) and a single-species check
  (`CreatureTemplate.Type.BigEel`, already in `KillListRegistry.All` labeled "Leviathan").

**A cross-gimmick dependency this surfaced -- since fixed, see below, not left standing:**
Ranger and Spearfisher originally only ever saw kills for a character that ALSO had the separate
"Kill List" gimmick assigned, since kill recording was Kill List's own gimmick-gated mechanism.

**One deliberate simplification, flagged rather than silently assumed:** Ranger's "in one run" is
implemented against a PERMANENT (whole-save-file) record, not anything scoped to a single
continuous life -- this mod has no existing "run boundary" concept to check against (kill records
don't reset on death either). Same spirit as this doc's own note on Massacre/Outlaw using "our
own tally, not vanilla's" -- revisit if a real run-boundary mechanism gets built later.

## Status update, 2026-09-16 (later still): kill tracking made global

Per the user's own architectural note ("the kill list gimmick is supposed to be more for forcing/
rewarding the character for doing all of these kills... the tracker... should be moved out as a
global effect, always tracking primal characters' kills"), the kill-recording mechanism moved
from `CreatureKillListGimmick.cs` into a new `PrimalKillTracker.cs` -- a global, mod-wide
mechanism tracking every Primal-slot character's kills unconditionally, with no roster
restriction at recording time (vanilla's own ~90 `CreatureTemplate.Type` values total, no
unbounded-growth concern). The cross-gimmick dependency noted above is gone: `RangerTracker`/
`SpearfisherTracker` now read `PrimalKillTracker` directly, independent of Kill List's own
assignment. `CreatureKillListGimmick.cs` is now purely a consumer -- its own 55-species curated
roster plus its own cycle-end button/dialog, reading from the same global record.

Also found and fixed a real Harmony-ordering bug while doing this: two independent Prefix patches
on the same vanilla method (`SurvivorEvents`' own Win hook and `PrimalKillTracker`'s commit hook)
have no relative execution-order guarantee, so a challenge tracker checking kills from inside
`OnSuccessfulHibernation` could have read stale data depending on which Prefix Harmony happened to
run first. Fixed by having `PrimalKillTracker.IsKilled`/`UniqueKillCount` both fold in still-
pending (not yet committed) kills directly, rather than trying to force an order.

Still entirely unbuilt/untested -- no .NET toolchain in this container.

## Status update, 2026-09-16 (later still): 5 more trackers -- 8 of 50 now implemented

**FoeTracker**, **MassacreTracker**, **MotherTracker**, **SleepoverTracker**, **PassagesTracker**
join Descension/Ranger/Spearfisher. Full trace in AI_CONTEXT.md; short version: Foe/Massacre
needed a genuinely new PrimalKillTracker capability ("what did this character kill THIS CYCLE",
not "ever"), which surfaced a real Harmony bug -- two independent Prefix patches on the same
vanilla method have no ordering guarantee -- fixed with this codebase's first use of
`[HarmonyPriority]`. Mother needed one new SurvivorShelterContents helper for a raw item-instance
count (eggs are an item type, not a creature type). Sleepover and Passages both reused existing
mechanisms outright, Passages via `Enum.GetValues` since `AchievementID` (unlike `SandboxUnlockID`)
is a plain C# enum.

**Deliberately NOT attempted this pass, and why** (revising the ratings table's own overly tidy
"now ~1-2" framing for a few of these): Visitor/Nomad/Journey/U-Turn need a "player entered this
room" event this mod doesn't have; Tasty/Leukocyte/Visions/Mycologist/Desperado need an "ate X" or
"eaten inside a den" detection mechanism that doesn't exist either; Outlaw needs real research
into Hunter's actual scoring formula (this doc's own table undersold it -- kills alone likely
aren't what "10k points" means); Collector needs the full SandboxUnlockID roster, which is an
ExtEnum (no `Enum.GetValues` shortcut the way Passages got); Devotee needs "same shelter across
cycles" tracking plus a firm definition of "Depths"; Pilgrimage's overall difficulty is still
dominated by cross-region carry tracking, not the color-variant check this doc's table implied
was the only blocker.

Still entirely unbuilt/untested -- no .NET toolchain in this container.

## Next proposed shared mechanics, ranked by leverage (2026-09-16, later still)

Requested directly after verifying Mother/Sleepover: a list of what's still needed across the
remaining 42 challenges, ranked by how many each unlocks.

| # | Mechanism | Unlocks | Notes |
|---|---|---|---|
| 1 | **Room/region transition event** -- "player entered room X" | Visitor, Nomad, Journey, U-Turn, Circuit, Criminal, Symbiote, Charged, Martyr, Pilgrimage, Enlightenment (~11) | The single biggest gap. Everything needing a hardcoded start/end room, a loop, or a route has no way to detect it yet. Needs confirming the right vanilla hook point (candidates: wherever `Player`'s own room changes, `Room.Loaded`, or similar). Once built, `SurvivorCheckpointRun` (already written, zero consumers) becomes usable immediately. |
| 2 | **"Ate/consumed X" event** | Visions, Mycologist, Desperado, Vegan, Chef (partial), Offering (partial) (~5-6) | No hook exists for "player ate this specific object" -- likely `Player.SwallowObject` or each consumable's own eaten-callback. |
| 3 | **Generalize the fall-death pattern to "died to creature type X in location Y"** | Tasty, Leukocyte | Cheap -- `SurvivorFallDeathHook`'s wasDead/isDead trick already proves the pattern; swap the terrain-impact check for a killTag-creature-type + den/shelter-location check. |
| 4 | **Wire up `SurvivorCreatureTracking`** (already built, zero consumers) | Chieftain, Fishtank, Guardian, Symbiote, Charged, Pilgrimage, Chef, Offering (~8) | Not new code -- just needs a tracker to use it. The "arrived in destination room/region" half depends on #1. |
| 5 | **Wire up `SurvivorTollState`** (already built, zero consumers) | Heist, Manslaughter, Elder, Usurper, Criminal (~5) | Same story. Criminal also needs #1. |
| 6 | **Passage-usage detection** | Enlightenment | Hook wherever a passage actually triggers travel. |
| 7 | **Non-lethal "demasking" detection** | Elder, Usurper | Distinct from a kill -- removing a mask while the creature survives. Needs its own trace through Vulture/King Vulture mask-drop code. |
| 8 | **"Same starting shelter across cycles" tracking** | Devotee | Echo-met tracking already solved (`SurvivorVanillaProgress.HasMetGhost`) -- this is the one missing piece. |
| 9 | **Neuron-carry tracking** (SSOracleSwarmer "follow" objects) | Enlightenment | Neurons aren't grabbed items -- a distinct follow-mechanic. |
| 10 | **Rot-stat reading** (Unfortunate Development) | Martyr | Single obscure stat, needs locating. |
| 11 | **Dream/DreamsState reading** | Despicable | Identify which field picks "Stolen Enlightenment" specifically. |
| 12 | **"Pinned to a surface" detection** | Ornaments | Needs the real state a spear-stuck creature carries. |
| 13 | **Enumerate the full `SandboxUnlockID` roster** (ExtEnum, no `Enum.GetValues` shortcut) | Collector | Needs reflection into `ExtEnumType`'s registered-values list -- same idea as `AllAchievements()`, harder since it isn't a plain enum. |
| 14 | **Real Hunter scoring formula** (food/survival/spear-hit, not just kills) | Outlaw | Research-only; `PrimalKillTracker`'s point table alone likely won't reach realistic thresholds. |
| 15 | **Guardian-visit trigger** | Guardian | A specific vanilla cutscene/trigger. |
| 16 | **Pole plant mechanics** | Flytrap | Never touched by this codebase; a hazard object, not a creature. |
| 17 | **Triple-spawn heuristic** | Triplopia | Inherently fragile -- indirect inference, not a clean state check. |
| 18 | **Communications Array laser/signal tracking** | Broadcast | Single-purpose, obscure system. |
| 19 | **Wire up existing `SurvivorEvents.OnGameEnding`** (zero-cost, already built) | Ascension, Perseverance | Not really a gap -- just hasn't been done yet. |

**Recommendation:** #1 is the clear next investment -- same shape as `SurvivorEvents` itself,
unlocking ~11 challenges directly and making two already-built-but-idle mechanisms
(`SurvivorCheckpointRun`, half of `SurvivorCreatureTracking`'s use cases) actually usable. #19 is
free and worth bundling into whichever batch comes next.

## Status update, 2026-09-16 (later still): 5 more shared mechanics realized (#1, #2, #3, #6, #7)

Requested directly: "realize 5 of the first and most useful shared mechanics not yet realized."
Selected the 5 highest-leverage genuinely-unbuilt mechanisms from the list above (#4 and #5 were
excluded on purpose -- both are already-built mechanisms with zero consumers, not "not yet
realized"). All 5 landed as new events on the existing `SurvivorEvents` bus in
`SurvivorEvents.cs`, following that file's own established pattern exactly (one Harmony patch
class per vanilla hook, each top-level, gated the same broad way the existing four events are).

- **#1, room/region transition** -- `SurvivorEvents.OnPlayerEnteredRoom`, a Postfix on
  `Player.NewRoom(Room)`. Confirmed via decompilation to be Player's own override of the virtual
  `PhysicalObject.NewRoom`, the same "just arrived" moment vanilla itself uses (tongue
  repositioning, MSC's item-tracker spawn logic). Hands back room name + region name as plain
  strings. This is the mechanism `SurvivorCheckpointRun` (built earlier, zero consumers) was
  waiting on -- it's now wireable.
- **#2, "ate X"** -- `SurvivorEvents.OnObjectEaten`, a Postfix on
  `Player.ObjectEaten(IPlayerEdible)`. Confirmed as the universal "fully consumed" hook via 14
  separate call sites across the decompiled source (every edible class calls it on itself once
  eaten). Hands back the raw `IPlayerEdible` -- that interface exposes no type info and spans both
  items and creatures (e.g. Hazer), so each subscriber pattern-matches for what it needs.
- **#3, generalized death-cause detection** -- `SurvivorEvents.OnPlayerDeath`, a Postfix on
  `Player.Die()` using the same wasDead/isDead transition trick `SurvivorFallDeathHook` already
  proved. Confirmed Player.Die() does NOT call `base.Die()` outside an MSC/Coop branch this mod
  doesn't target, so this is the only hook that actually sees a player's own death (separate from
  `PrimalKillDieHook`'s patch on the base `Creature.Die`). Hands back killer creature type (off
  the same `PrimalKillTracker.KillTagField` handle, not a second reflection), plus `InDen` and
  `AbstractRoom.shelter` flags for Tasty/Leukocyte's location checks.
- **#6, passage usage** -- `SurvivorEvents.OnPassageUsed`, a Prefix/Postfix pair on
  `Menu.SleepAndDeathScreen.Singal`, gated on the exact `"PASSAGE"` case body (only fires if
  `proceedWithEndgameID` actually transitioned from null to non-null, i.e. a real passage was
  available and taken). **Deliberate simplification, flagged on purpose:** this is a
  conservative proxy for "used a passage" -- it can't distinguish which specific passage, only
  that one was taken -- which happens to be the stricter, safer reading for Enlightenment's
  "without ever passaging" rule anyway.
- **#7, non-lethal demasking** -- `SurvivorEvents.OnNonLethalDemask`, a Postfix on
  `Vulture.Violence` comparing mask-before/mask-after/dead-after (not a patch on
  `Vulture.DropMask` itself, which runs too early in `Violence`'s own body to know if that same
  hit was fatal). Attribution reuses `PrimalKillTracker.KillTagField`: `Creature.SetKillTag` is
  called by weapon-hit resolution on every connecting hit, not just fatal ones, decaying after
  200 ticks if the creature survives -- already the same trustworthy "who last hit this" signal
  kill-tracking relies on. **Known gap, flagged on purpose:** this only covers the
  mask-falls-off-a-live-Vulture mechanic (Usurper). Elder's "6 Scavengers wearing Vulture Masks"
  is a Scavenger-equips-an-item mechanic with no relationship to this event -- still a genuine gap,
  not touched by this batch.

**Nothing here has been built or tested** -- no .NET toolchain in this container. Every vanilla
member used (`Player.NewRoom`, `Player.ObjectEaten`, `Player.Die`, `AbstractWorldEntity.InDen`,
`Menu.SleepAndDeathScreen.Singal`/`proceedWithEndgameID`, `Vulture.Violence`,
`Vulture.VultureState.mask`, `Creature.SetKillTag`) was verified against
`reference/GameSource/*.cs` before use, matching this project's standing rule. Brace/paren balance
re-checked across the whole `src/Primal` tree after this change.

Zero consumers wired up yet for any of these 5 -- same "mechanism only" scope as the original
8-mechanism batch. Gap-list items #4, #5, and #8-#19 above are still open.

## Status update, 2026-09-16 (later still): original 50-challenge table recovered as CSV; 6 more trackers wired -- 14 of 50 now implemented

Requested: "wire up the trackers for whom the shared mechanics already exist for." Hit a real
blocker first -- the condensed "what it needs" notes in this doc's own ratings table describe
*mechanisms*, not always the concrete data (exact creature types, room names, thresholds) needed
to implement a specific challenge correctly, and that data only ever existed in the original
50-row table pasted in chat, which had since fallen out of context. Asked the user directly rather
than guessing; they re-pasted it, now saved verbatim as `community_challenges_source.csv` (23
columns: name, difficulty, exact challenge description, PC/console verification requirements,
notes, vote history) for this and every future batch to reference.

With exact wording available, picked the 6 challenges buildable purely from mechanisms that
already exist -- no new research, matching the request's own scope -- and skipped everything
still needing a genuinely new mechanism (cross-region creature-carry, color/variant roster,
toll/reputation state wiring, room-name data-entry for challenges like Nomad/Journey/Circuit/
Desperado where the exact room IDs aren't in the CSV either and would need separate lookup):

- **TastyTracker** and **LeukocyteTracker** -- both direct reads of the new
  `SurvivorEvents.OnPlayerDeath` (killer creature type + den/shelter flags), exactly the
  mechanism gap #3 was built to close. Tasty counts all three vanilla centipede types
  (`Centipede`, `SmallCentipede`, `RedCentipede`) as satisfying "a centipede" since the challenge
  text doesn't specify one -- a judgment call, flagged in the class's own doc comment.
- **VisionsTracker** (10 Karma Flowers in one cycle) and **MycologistTracker** (a Mushroom from 4
  distinct regions on the starting cycle) -- both direct consumers of the new
  `SurvivorEvents.OnObjectEaten`, each with its own small per-character, per-cycle counter/set
  reset at cycle boundaries. Visions' "No Hunter Flower!" exclusion isn't checked (no vanilla flag
  for it found on a first pass) -- flagged as a known gap rather than silently ignored.
- **UsurperTracker** -- the first consumer of the new `SurvivorEvents.OnNonLethalDemask`: a
  per-cycle count of King-Vulture-specific demasks (>= 2) plus a shelter check for 2+ King Vulture
  masks physically present at hibernation (`VultureMask.AbstractVultureMask.king`, a plain public
  bool, trusted directly). "No Echo pacification!" left as an honor-system note.
- **PerseveranceTracker** -- the first consumer of `SurvivorEvents.OnGameEnding` (gap-list item
  #19, flagged as "zero-cost, already built" since the original 8-mechanism batch). Reuses the
  same `standardVoidSea` flag SlugpupGimmick's own ending hook already relies on. "Starting on
  cycle zero" and "no Guardian skip" are both Remix-assist-configuration/honor-system clauses per
  the CSV's own Notes column, not in-run state this mod can check -- left unenforced, same
  treatment Saint/Vegan's own unenforceable clauses already get in this doc.

**Still not built or tested in-game** -- no .NET toolchain in this container. Brace/paren balance
re-checked across the whole `src/Primal` tree after this change. 14 of 50 challenges now have a
working tracker (assuming correctness once actually tested); 36 remain, most blocked on a
not-yet-built mechanism (cross-region carry, toll/reputation state, color/variant roster, several
single-purpose research items) rather than missing data now that the CSV exists.

## Status update, 2026-09-16 (later still): cross-region carry, toll state, and color variant mechanisms implemented

Requested: "implement shared mechanics for cross region creature carry, toll state and color
variant identification" -- the three biggest remaining blockers from the shared-mechanisms list
(items #3, #5, #4). Mechanism-only, same scope as every earlier shared-mechanics batch -- no new
trackers wired up this pass.

- **Cross-region carry (`SurvivorCreatureTracking.cs`)** -- the identity-tracking half
  (Touch/Untrack/AllTrackedAlive/FindInRoom) already existed with zero consumers; the actual
  missing piece was "is the player physically holding this creature right now," closed with one
  new method, `IsBeingCarriedBy(Player, AbstractCreature)`, reading `Creature.grasps` (confirmed
  public) the same way `PrimalKillTracker`'s own Grabbed hook already reads it from the grabber's
  side. Combined with the already-built `SurvivorEvents.OnPlayerEnteredRoom`, this is now
  sufficient for every carry-shaped challenge (Symbiote, Charged, Offering, Chef, Guardian,
  Pilgrimage): track the creature's identity, check `IsBeingCarriedBy` + `AllTrackedAlive` at each
  room arrival. Chieftain/Pilgrimage's own "escort, not carry" cases (a tamed Scavenger/Lizard
  that follows on its own) needed no NEW mechanism at all -- identity-tracking + room-arrival
  alone already answers those.
- **Toll state (`SurvivorTollState.cs`, `SurvivorTollRoster.cs`)** -- closed the "stole vs.
  attacked" gap this file's own header comment previously flagged as unresolved, plus two more
  pieces. Traced `ScavengerReportTransgression`'s real call sites into
  `ScavengerAI.SocialEvent(EventID, Creature, Creature, PhysicalObject)`, which has both the
  specific event type and the actual item involved -- added `OnItemStolen` as its own independent
  signal off `SocialEvent` (`EventID.Theft`) rather than piggybacking on the coarser transgression
  hook, for Heist's "stole a pearl" need. Added `OnTollPassed` (a second, independent Postfix on
  `Player.NewRoom`) for Criminal's "passed 3 tolls without paying." Built a brand-new
  `SurvivorTollRoster.cs` for Manslaughter specifically: `ScavengerOutpost.team` only ever holds
  CURRENTLY-tracked guards (confirmed via decompilation -- dead ones are removed the same tick,
  replacements just get appended), so it can never by itself answer "is everyone who was EVER
  assigned here now dead." The new file keeps its own persistent roster (keyed on the outpost's
  own room name) fed by a Postfix on `ScavengerOutpost.Update`, plus a separate Postfix on
  `Creature.Die` that flags an outpost as disqualified the instant the player gets kill credit for
  one of its guards (reusing `PrimalKillTracker`'s own killTag/handler attribution).
- **Color/variant identification (`SurvivorCreatureVariant.cs`)** -- confirmed
  `CreatureTemplate.Type.SpitterSpider` is its own distinct registered Type, closing out Safari's
  third creature (all three of Red Lizard/Red Centipede/Spitter Spider are now plain Type
  comparisons, no new mechanism needed). Re-examined Lantern Mouse's purple/green question with
  the actual generation formula this time (`LanternMouse.GenerateIVars`): it's a continuous hue
  with two probability clusters (a cyan-through-violet arc ~50% of the time, red/orange ~50% of
  the time) and no dedicated green cluster at all outside a rare 1% fully-random roll -- confirmed
  this genuinely isn't a clean two-value split, not just "not yet researched." Left deliberately
  unimplemented, same honor-system treatment as before, now with the real formula on record
  instead of a placeholder note.

**Still not built or tested in-game** -- no .NET toolchain in this container. Every vanilla member
used (`Creature.grasps`, `ScavengerAI.SocialEvent`/`outpostModule`, `ScavengerOutpost.Update`/
`team`, `CreatureTemplate.Type.SpitterSpider`, `LanternMouse.GenerateIVars`) was verified against
`reference/GameSource/*.cs` first. Brace/paren balance re-checked across the whole `src/Primal`
tree after this change. Still open: Scholar's own pearl-color-roster enumeration (a data-entry
task, not a variant-identification one) and every remaining single-purpose research item from the
original 19-item gap list.

## Status update, 2026-09-16 (later still): color-variant trackers -- Feast and Safari, 16 of 50 now implemented

Requested: "implement the color variant mechanic needed trackers." Of the six challenges the
color-variant mechanism was originally rated against (Feast, Safari, Sleepover, Offering, Chef,
Pilgrimage), Sleepover was already implemented earlier. Checked the other four against the exact
CSV wording:

- **Feast** and **Safari** were genuinely blocked ONLY on the variant check itself, now resolved
  (RedCentipede/SpitterSpider both confirmed distinct `CreatureTemplate.Type` values) -- both
  implemented this pass.
- **Offering** and **Chef** and **Pilgrimage** are NOT actually unblocked by this mechanism alone,
  despite being on the original list:
  - Offering needs the Lantern Mouse purple/green check, which `SurvivorCreatureVariant.cs`
    deliberately leaves unimplemented (see that file's own doc comment -- confirmed this pass to
    be a genuine continuous-value judgment call, not just unresearched), AND a wholly separate,
    unbuilt "fed a carried creature to a predator" event. Skipped.
  - Chef's albino-Jetfish check is done, but "kill it on a zapper" needs a specific
    death-by-hazard-object detection this mod has never built (a Zapper is an environmental
    object, not a `Creature`, so the killTag-based `OnPlayerDeath`/kill-attribution mechanisms
    don't apply). Skipped.
  - Pilgrimage's own Cyan Lizard check is trivial (already a confirmed distinct Type, already
    used by `FoeTracker`), but the route itself ("Memory Crypts, The Leg and Underhang") doesn't
    resolve to real region acronyms -- checked the authoritative source this pass
    (`Menu/RegionSelectMenu.cs`'s own debug region list: SU/HI/DS/CC/LF/SI/SH/GW/SL/UW/SB/SS, with
    UW = "THE WALL", confirming Pilgrimage's own "No climbing The Wall!" note lines up with a real
    region on the route) -- but "Memory Crypts," "The Leg," and "Underhang" are none of these 12
    official names, so they must be community nicknames for specific SUB-areas, not resolvable
    without further research. Same category of gap as Nomad/Journey/Circuit/Desperado/U-Turn/
    Visitor from the earlier batch (room/sub-area name data-entry, not a missing mechanism).
    Skipped.

**FeastTracker** ("kill AND eat the SAME Red Centipede") needed one genuinely new small piece:
"Lineage" means the specific creature instance matters, not just "killed a Red Centipede and
separately ate a different one" -- every existing kill record in `PrimalKillTracker` is per-TYPE,
which can't answer that. Added `RecordInstanceKill`/`WasInstanceKilledBy` to `PrimalKillTracker`
itself (populated from the SAME existing `PrimalKillDieHook` Postfix, no new patch needed), keyed
on `EntityID.number` per the usual gotcha. `Centipede` implements `IPlayerEdible` directly (its
own corpse is what eventually reports being eaten), so cross-referencing `OnObjectEaten`'s edible
against that instance record is a direct correlation, not a guess.

**SafariTracker** ("Red Lizard, Red Centipede and Spitter Spider alive on the same screen, then
sleep") checks the player's current room contents directly (not `SurvivorShelterContents`, which
is shelter-only) once per room arrival (`SurvivorEvents.OnPlayerEnteredRoom`), setting a per-cycle
"seen together" flag checked at the next successful hibernation. **Known simplification:** only
re-checked on the player's own room changes, not continuously -- a third creature wandering into
an already-occupied room without the player also changing rooms won't be caught until the
player's next transition.

16 of 50 challenges now have a tracker registered. **Still not built or tested in-game** -- no
.NET toolchain in this container. Brace/paren balance re-checked across the whole `src/Primal`
tree after this change.

## Status update, 2026-09-16 (later still): the last 3 -- Pilgrimage, Chef, Offering; 19 of 50 now implemented

Requested: "implement mechanics needed for these 3 unimplemented trackers," with three pieces of
user-supplied research that unblocked each: "Memory Crypts" is a subregion of Shaded Citadel,
"The Leg"/"Underhang" are subregions of The Exterior; for Chef, check for a nearby/in-room Zapper
object at the moment of death; for Offering, the hue is findable, and "fed to a predator" likely
hooks the same code as taming/Lizard-reputation increases.

- **Pilgrimage** -- confirmed the region mapping via the authoritative source
  (`Menu/RegionSelectMenu.cs`'s own debug region list): Shaded Citadel = SH, The Exterior = UW
  (matching the challenge's own "No climbing The Wall!" note). Since Primal has no sub-area data
  below the whole-region level, tracked at region granularity: carry a live Cyan Lizard from
  Outskirts through SH and UW (either order) to arrival in Five Pebbles (SS). No new mechanism
  needed -- just `SurvivorCreatureTracking` + `IsBeingCarriedBy` + `OnPlayerEnteredRoom`, all
  already built.
- **Chef** -- the user's own hunch about zapper-proximity was correct AND necessary: decompiling
  `ZapCoil.cs` confirmed its own `Update` body kills a creature with a DIRECT `Creature.Die()`
  call, completely bypassing `Violence`/`SetKillTag` -- there is no killer to attribute this to at
  all, so the existing killTag-based mechanisms could never have seen this cause regardless of how
  much wiring was added to them. Built a new `SurvivorHazardDeath.cs`: checks `Room.zapCoils`
  (confirmed public) for an active coil (`turnedOn > 0.5f`, the exact threshold `ZapCoil.Update`
  itself gates real zapping on) in the room a creature died in. Flagged as an imprecise proxy (a
  creature dying of an unrelated cause in the same room as an active coil would also match) --
  acceptable for Chef's own deliberate, curated setup.
- **Offering** -- the user's hunch about the taming/reputation code was directionally right (that
  code exists, `SocialEventRecognizer.ItemOffered`/`EventID.ItemOffering`, used by
  `LizardAI`/`CicadaAI`/`ScavengerAI`) but doesn't apply to Leviathans specifically: confirmed via
  decompiling `BigEelAI.cs` that it does not implement `IReactToSocialEvents` at all, so
  `SocialEventRecognizer`'s own dispatch loop never reaches a BigEel no matter what's offered to
  it. The real mechanism is `BigEel.Swallow()` -- traced via decompilation to be the one method
  that actually consumes whatever's caught, entirely independent of the social-event system. Built
  a new `SurvivorPredation.cs` around it. For the color half, the user asked directly to stop
  deferring the Lantern Mouse hue question -- `SurvivorCreatureVariant.IsPurpleOrGreenLanternMouse`
  now buckets the already-traced hue formula into a "purple" band (the formula's own common
  cool-color cluster) and a "green" band (the standard HSL green range, reachable only via the
  formula's rare 1% fully-random roll) -- explicitly still a judgment call, not a rediscovered
  certainty, but implemented per the user's own direction rather than left honor-system.

All three carry-tracking trackers (Pilgrimage, Chef, Offering) share the same shape: start
tracking the first time the character is seen carrying a matching creature in the right starting
region, abandon if ever not-carried or dead, complete on the relevant terminal event. Chef and
Offering both needed a small reverse index (creature instance -> character) since their terminal
events (`OnZapCoilDeath`, `OnLeviathanSwallow`) only hand back the affected creature, not a
player -- the predator kills/eats autonomously, so there's no player-side hook at that exact
moment otherwise.

19 of 50 challenges now have a tracker registered. **Still not built or tested in-game** -- no
.NET toolchain in this container. Every vanilla member used (`Room.zapCoils`, `ZapCoil.turnedOn`,
`BigEel.Swallow`/`clampedObjects`, `BigEelAI` not implementing `IReactToSocialEvents`,
`LanternMouse.iVars.color.hue`, `Menu/RegionSelectMenu.cs`'s region list) was verified against
`reference/GameSource/*.cs` first. Brace/paren balance re-checked across the whole `src/Primal`
tree after this change.

## Status update, 2026-09-16 (later still): Ornaments implemented; 4 more gaps researched, need user input

Requested: "go through 5 more gaps" from the 19-item shared-mechanics list (items #8-18, the
remaining ones after #1-7 and #19 were all closed in earlier batches). Researched 5; one resolved
cleanly from decompiled source alone, four hit genuine ambiguity that needs either a design
decision or gameplay knowledge this doc's own research pass can't settle from source code alone --
flagged explicitly rather than guessed.

- **Ornaments (gap n/a -- "pinned to a surface" detection, item #12) -- IMPLEMENTED.** Confirmed
  via decompiling `Spear.LodgeInCreature`: a thrown spear with a high enough damage bonus, hitting
  a creature with a solid wall directly behind it, sets `stuckInWall` (a `Vector2?`) and
  `pinToWallCounter = 300` -- genuinely pinning the creature to that wall. Both plain public
  fields. Built a new `SurvivorPinning.cs` (`OnCreaturePinnedToWall`, a Prefix/Postfix pair on
  `LodgeInCreature` using the usual wasX/isX transition trick) and `OrnamentsTracker` (12 distinct
  live-at-pin-time creature instances in one cycle -- "unique" read as distinct individuals, not
  distinct species, since the CSV notes never mention species diversity the way Foe's own wording
  does).
- **Collector (item #13, SandboxUnlockID roster) -- mechanism found, scope unclear.** Confirmed
  `ExtEnum<T>.values.entries` is a plain public `List<string>` -- the FULL registered-value list
  is directly readable with no reflection needed at all (better than expected; `AllAchievements()`
  -style `Enum.GetValues` wasn't even necessary). The open question is scope: does "all arena,
  creature and item Sandbox unlocks" mean literally every registered `SandboxUnlockID`, or a
  curated subset (the challenge names three categories specifically, which could imply excluding
  some other kind of unlock this doc hasn't identified)? Needs either the user's own knowledge of
  the Sandbox unlock system, or a decision to just implement "all of them" as a reasonable
  approximation.
- **Devotee (item #8, same-shelter-across-cycles) -- half resolved.** `GhostWorldPresence.GhostID`
  is confirmed a small, fixed, compile-time-known ExtEnum roster (CC, SI, LF, SH, UW, SB -- exactly
  6 real regions, plus a `NoGhost` sentinel), and `SurvivorVanillaProgress.HasMetGhost` already
  exists to check each. What's missing: what "reach the Depths" corresponds to mechanically (a
  specific room/region, or some other vanilla flag) -- needed before the "one shelter across all
  these cycles" tracking can be finished, since the final Depths cycle's own starting shelter has
  to match too.
- **Martyr (item #10, rot-stat reading) -- likely wrong mechanism, needs confirmation.** Found
  `RegionState.sentientRotProgression` (a `Dictionary<string, RotState>` with a `rotIntensity`
  float, gated behind `miscWorldSaveData.hasVoidWeaverAbility`) -- a real vanilla "Sentient Rot"
  mechanic, but tied to a specific late-game ability flag that may not even apply to an ordinary
  Unfortunate Development visit. Genuinely unsure this is the same "rot" the challenge means by
  "reach a Memory Conflux room without rot" -- could be an unrelated, more local hazard specific to
  that room. Needs the user's own knowledge of Unfortunate Development's actual mechanics.
- **Guardian (item #15, Guardian-visit trigger) -- likely wrong class, needs confirmation.**
  Confirmed the real internal name for what the community calls "Guardians" is `TempleGuard` (a
  normal `Creature` with its own `TempleGuardAI`) -- but the challenge's own "take them both to
  visit the Guardians" phrasing implies something more specific than mere proximity (Rain World's
  TempleGuards can reportedly be appeased/passed via a specific interaction). Needs the user's own
  knowledge of what "meeting the Guardians" actually requires mechanically.

20 of 50 challenges now have a tracker registered. **Still not built or tested in-game** -- no
.NET toolchain in this container. Brace/paren balance re-checked across the whole `src/Primal`
tree after this change.

## Status update, 2026-09-16 (later still): Collector, Devotee, Martyr, Guardian implemented; 24 of 50 now done

The user answered all 4 open questions from the previous batch directly, unblocking every one of
them:

- **Collector**: "all means all that are collectible by either monk, survivor or hunter... select
  all of the unlockables for sandbox, arena and safari that are intended for vanilla." Confirmed
  `ExtEnum<T>.values.entries` is a plain public `List<string>` -- the full registered
  `SandboxUnlockID` roster, no reflection needed. With MSC disabled, only vanilla's own
  registrations ever run, so this comes back correctly scoped with no extra filtering. Added
  `SurvivorVanillaProgress.AllSandboxUnlockIDs()` and `CollectorTracker` (checked at hibernation,
  same shape as `PassagesTracker`).
- **Devotee**: "reaching the Depths" = arriving in room `SB_E05`. Rather than reactively reading
  the permanent `HasMetGhost` flag (which can't tell an Echo met long ago from one met just now,
  and never resets), built a new `SurvivorEchoEncounter.cs` hooking `SaveState.GhostEncounter`
  directly (confirmed via decompilation to unconditionally set `ghostsTalkedTo[ghost] = 2` right
  at the top of its own body) for a genuinely LIVE per-Echo signal. `DevoteeTracker` locks in a
  "home shelter" on each hibernation, resets the whole attempt if a hibernation happens from a
  different shelter (matching "all from one shelter"), and only credits Echo encounters to
  whichever attempt is currently active -- naturally handling "does not need to be completed in
  consecutive successful cycles" (a death doesn't touch the shelter-chain state at all).
- **Martyr**: the rot-less Memory Conflux rooms are `SS_LAB7` and `SS_A01`. `MartyrTracker` checks
  the whole cycle stays inside Five Pebbles and touches one of those two rooms before ending in a
  successful hibernation. **Known simplifications, flagged in the tracker's own doc comment:**
  doesn't verify the exact starting shelter or that the return route specifically passes back
  through Unfortunate Development -- neither UD's own room list nor the starting shelter's room ID
  were given this round; happy to tighten this further if that data comes up later.
- **Guardian**: the Guardians room is `SB_D03`; "Grapple Worm" is vanilla's own `TubeWorm`
  (confirmed via its own `grappleToPos`/`GrappleToPosition()` fields -- the exact zipline/grapple
  mechanic the nickname refers to). `GuardianTracker` tracks both the Worm and the Snail
  simultaneously under one `SurvivorCreatureTracking` key (that class's existing multi-creature
  support needed no changes at all), reads `Player.Malnourished` on each room transition for the
  "without starving" clause, and completes on arrival at `SB_D03` with both still alive and
  carried. **Known simplification:** doesn't verify the "highest room in Communication Arrays"
  waypoint (its room ID wasn't given this round either).

24 of 50 challenges now have a tracker registered. **Still not built or tested in-game** -- no
.NET toolchain in this container. Every vanilla member used (`ExtEnum<T>.values.entries`,
`SaveState.GhostEncounter`, `Player.Malnourished`, `TubeWorm.grappleToPos`) was verified against
`reference/GameSource/*.cs` first. Brace/paren balance re-checked across the whole `src/Primal`
tree after this change.

## Status update, 2026-09-16 (later still): Despicable implemented; 5 more mechanics researched, need user input

Requested: "focus on the missing shared mechanics." Researched the 6 remaining mechanism-level
gaps from the original 19-item list (Enlightenment's neuron carry, Despicable's dream state,
Outlaw's scoring formula, Flytrap's pole plant, Triplopia's triple-spawn heuristic, Broadcast's
comms array) -- one resolved cleanly, five hit real ambiguity or missing pieces flagged explicitly
rather than guessed past.

- **Despicable -- IMPLEMENTED.** Confirmed `DreamsState.DreamID.MoonThief` is "Stolen
  Enlightenment" (chosen when `SLOracleState.neuronsLeft < 5` -- i.e. the player took 5+ of Moon's
  neurons, the "stole from Moon" story this dream depicts; its sibling `MoonFriend` fires the
  opposite way). Built a new `SurvivorDreamState.cs`. **Real timing gotcha found and documented:**
  this had to be the first-ever Postfix (not Prefix) on `RainWorldGame.Win` in this whole event
  bus, since the dream only finalizes partway through `Win`'s own body -- but that means
  `SaveState.cycleNumber` has ALREADY incremented by the time this fires, so "starting cycle" here
  checks `== 1`, not `== 0` like every other Win-Prefix-timed check in this codebase. Flagged
  loudly in both the event's own doc comment and `DespicableTracker`'s, since getting this
  backwards would silently detect the wrong cycle.
- **Enlightenment (neuron-carry) -- genuinely confusing, needs clarification.** Two candidate
  classes exist: `SSOracleSwarmer` (a graspable `PhysicalObject` implementing `IPlayerEdible` --
  literally holdable like a Rock, confirmed via its own `Edible` property rejecting consumption
  only when the player's stomach is already full) and `NSHSwarmer` (a separate class tied to a
  scripted Pebbles cutscene). Neither obviously matches "carry 7 things across two regions" the
  way this mod's other carry challenges do, since a Player can only hold 2 items in hand at once --
  the CSV's own "Total neurons at the end of the challenge must be 12 or more" note suggests
  there's a real, PERSISTENT vanilla delivery counter behind this (accumulated across multiple
  trips), not a live 7-at-once carry, but no such counter was found in a first search.
- **Outlaw (Hunter scoring) -- likely needs a simplification decision.** No real vanilla
  "Hunter score" field or score-screen class exists in the decompiled source at all -- the CSV's
  own "Score calculator" link and "standard scorerunning rules" note confirm this is the
  community's OWN external scoring convention, not anything vanilla computes or displays. This
  mod's existing kill-point table (`SurvivorVanillaProgress.KillPointValue`, already used by
  Massacre/Ranger) could sum kills across a whole run, but per this doc's own earlier note, real
  competitive Hunter scoring also counts food/survival/spear-hits -- kills alone would likely
  undercount versus the "real" 10k threshold.
- **Flytrap (pole plant) -- class found, exact mechanic unclear.** `PoleMimic` IS the real "pole
  plant" (confirmed `Creature`, not a hazard object as this doc previously assumed) -- it grabs
  nearby creatures via its own ambush logic (`Grab(...)`, `Vulture.cs`'s own confirmed prey
  relationship checks). Unclear: exactly how a player "feeds" a live Vulture to it (does the
  Vulture just need to wander/be dragged into grab range, or is there a specific technique), and
  what state confirms a successful "fed and player survived" outcome versus any other grab.
- **Triplopia (triple-spawn heuristic) -- likely genuinely undetectable, leaning honor-system.**
  This is a known community EXPLOIT technique (the CSV's own "Multi-spawning documentation" link
  points to external material), not a normal vanilla mechanic -- this doc's own earlier note
  already called it "inherently fragile." No obvious save-data signature was found to key off.
- **Broadcast (Communications Array signal) -- nothing found at all.** No "laser," "signal," or
  "array" reference anywhere in the decompiled source, and no dedicated room-effect type for it
  either (unlike `ElectricDeath`, which WAS findable this way). Genuinely stuck on where this
  mechanic even lives in vanilla's own code.

**Still not built or tested in-game** -- no .NET toolchain in this container. 25 of 50 challenges
now have a tracker registered. Brace/paren balance re-checked across the whole `src/Primal` tree
after this change.

## Status update, 2026-09-16 (later still): Enlightenment and Flytrap implemented; Outlaw/Broadcast still stuck, Triplopia deferred

The user answered all 4 remaining questions. Two unblocked cleanly; Triplopia is explicitly
deferred; Outlaw still needs a source location this pass couldn't find.

- **Enlightenment -- IMPLEMENTED.** "SL_AI contains the Looks to the Moon iterator entity and if
  you throw a neuron to her, she stores them." Confirmed via decompilation:
  `SLOracleBehavior.ConvertingSSSwarmer()` increments `SLOrcacleState.neuronsLeft` -- the SAME
  persistent field `DespicableTracker`'s own MoonThief detection already reads, confirming this is
  one real, continuously-updating counter (not a per-attempt tally this mod invents). Checked as a
  plain `neuronsLeft >= 12` threshold, matching the CSV's own explicit numeric requirement exactly.
  "Without passaging" is treated as a PERMANENT, save-file-wide disqualification -- required
  extending `SurvivorEvents.OnPassageUsed`'s own signature to also hand back
  `DeathPersistentSaveData` (nothing else consumed that event yet, so this was a safe, cheap
  extension rather than a second event).
- **Flytrap -- IMPLEMENTED.** "Anger the pole mimic so it is in its active state and then have a
  vulture go over it." Confirmed via decompilation: `PoleMimic.Collide` contains exactly this
  condition (`Consious && WakeUp > 0.3f`, the real state behind "angered/active") gating its own
  ambush `Grab(...)` call. Built `SurvivorPoleMimicPredation.cs` around this. **Known
  simplification, flagged in the tracker's own doc comment:** no explicit prey-killing method was
  found on `PoleMimic` (unlike `BigEel.Swallow`'s clean kill call) -- this tracker treats the GRAB
  ITSELF as "fed" rather than waiting for a confirmed kill, and "survive the encounter" as "player
  not currently dead at that moment."
- **Triplopia -- deferred per the user's own direction** ("there may not be a proper technical
  signature... put this away for now as one of the last trackers to implement"). Not built.
- **Outlaw -- still unresolved.** The user described a real formula (deaths/quits subtract; food
  eaten, survived cycles add; multiplied by total passages acquired), present in Hunter's own
  "ran out of cycles, died at cycle 0" ending. Traced `WinState.CycleCompleted` (the method behind
  the real, in-game `EndgameID.Outlaw` PASSAGE token -- confirmed Outlaw is also a genuine vanilla
  passage, not just a community challenge name) and found its own Outlaw-tracker contribution is
  just `num2` -- a per-cycle count of kills where `CreatureTemplate.countsAsAKill > 1` -- nowhere
  close to the richer formula described. Searched `RedsIllness.cs` (Hunter's cycle-limit mechanic)
  and broadly for "quit count," "survival score" and similar terms with no match. Still open:
  either this formula lives somewhere this pass's search terms missed, or it's a community-only
  calculation done from raw end-of-run stats rather than anything vanilla computes as one number.

27 of 50 challenges now have a tracker registered. **Still not built or tested in-game** -- no
.NET toolchain in this container. Every vanilla member used (`SLOracleBehavior
.ConvertingSSSwarmer`, `SLOrcacleState.neuronsLeft`, `PoleMimic.Collide`/`Consious`/`WakeUp`) was
verified against `reference/GameSource/*.cs` first. Brace/paren balance re-checked across the
whole `src/Primal` tree after this change.

## Status update, 2026-09-16 (later still): Manslaughter, Chieftain, Criminal wired up; 30 of 50 now implemented

Requested: "put Outlaw aside next to Triplopia, continue implementing next features." Wired up
three toll/carry-adjacent challenges whose underlying mechanisms already existed from earlier
batches (SurvivorTollState, SurvivorTollRoster, SurvivorCreatureTracking) -- no new mechanisms
needed, just consumers.

- **ManslaughterTracker** -- the first consumer of `SurvivorTollRoster`. All three of its
  conditions (every guard ever assigned to this outpost dead, zero player kill credit, zero
  hostility ever triggered) were already directly answerable; only a small local "ever went
  hostile" set (fed by `SurvivorTollState.OnOutpostTransgression`) needed adding at the tracker
  level. No cycle-scoping in the challenge's own wording, matching the roster's own already-
  persistent (not per-cycle) design.
- **ChieftainTracker** -- "Bring live Scavengers to the Looks to the Moon subregion." The target
  room, `SL_AI`, fell out of the Enlightenment research from the previous batch (it's the same
  room Moon's own Oracle behavior runs in) -- lines up with the challenge's own note about a Karma
  Gate into Shoreline (SL). Scavengers here are tamed allies that follow the player, not carried
  items, so this is the first consumer of `SurvivorCreatureTracking`'s identity layer WITHOUT
  `IsBeingCarriedBy` at all. **Known simplification:** requires 2+ Scavengers (the challenge's own
  text just says "Scavengers," plural, with no explicit count anywhere in its CSV row) -- a
  judgment call, flagged in the tracker's own doc comment.
- **CriminalTracker** -- "Travel to the Garbage Wastes Stronghold after passing three Scavenger
  Tolls without paying on the starting cycle." Direct consumer of `SurvivorTollState.OnTollPassed`
  (paid == false), deduplicated by outpost room key. **Known simplification:** doesn't verify
  arrival at the actual "Garbage Wastes Stronghold" room -- its exact room ID wasn't available --
  completes once the 3rd distinct unpaid toll is passed while the whole attempt stays inside
  Garbage Wastes during the starting cycle.

Heist and Elder remain unwired: Heist needs the exact "lowest Garbage Wastes toll" identified
among possibly several GW outposts (not just any one), and Elder needs the still-unbuilt
Scavenger-wears-a-mask detection (a different mechanic from the demasking event already built for
Usurper). Both stay open rather than force a much weaker approximation.

30 of 50 challenges now have a tracker registered. **Still not built or tested in-game** -- no
.NET toolchain in this container. Brace/paren balance re-checked across the whole `src/Primal`
tree after this change.

## Status update, 2026-09-16 (later still): Nomad, Circuit, Desperado, U-Turn, Symbiote, Charged wired up; 36 of 50 now implemented

The user gave exact room IDs for 6 of the 9 room-name-blocked/carry challenges, explicitly parking
Journey, Visitor, and Fishtank as hard tasks for later.

- **NomadTracker** -- the 4 Industrial Complex Karma Gates: `GATE_HI_GW`, `GATE_SU_HI`,
  `GATE_HI_CC`, `GATE_HI_SH`. Completes LIVE the instant the 4th distinct gate room is entered, no
  hibernation needed, matching the CSV's own "You do not need to hibernate!" note exactly.
- **CircuitTracker** -- start/end shelter `SS_S05`; checkpoints (`SS_D04`, `SS_I03`, `SS_A03`) were
  already known from the original CSV's own Console Verification column. The first real consumer
  of `SurvivorCheckpointRun`, built during the very first shared-mechanisms batch and never used
  until now.
- **DesperadoTracker** -- "Deer Pearl room" is `LF_D07`. Confirmed "Popcorn Plant" is the
  community nickname for vanilla's `SeedCob` (`SlugNPCAI.GetFoodType` explicitly maps it to
  `Food.Popcorn`) -- but `SeedCob` is never eaten through `Player.ObjectEaten` at all, so a new
  file, `SurvivorSeedCobEaten.cs`, patches `PlayerSessionRecord.AddEat` directly (the real,
  universal "ate an external food source" signal this specific object uses instead).
- **UTurnTracker** -- northeast Outskirts shelter is `SU_S04`. Same shape as Circuit but with zero
  named checkpoints; `SurvivorCheckpointRun.AllCheckpointsHit` is vacuously true for an empty
  checkpoint list, so this needed no special-casing at all -- exactly the "no named checkpoints"
  case that class's own original design comment already anticipated.
- **SymbioteTracker** -- westernmost Underhang shelter `UW_S01`, shelter at the top of The Leg
  `UW_S07`. Confirmed "squidcada" is the community nickname for vanilla's Cicada (both
  `CicadaA`/`CicadaB` accepted, since the challenge doesn't specify which). "While starving" is
  checked continuously at every room transition during the carry, not just once.
- **ChargedTracker** -- eastmost Drainage System shelter `DS_S02`. "Baby centipede" is
  `CreatureTemplate.Type.SmallCentipede` specifically (the "youth" size class).

36 of 50 challenges now have a tracker registered. **Deferred, per the user's own explicit
direction, as hard tasks for later:** Journey (needs the exact "highest shelter in Five Pebbles"
room), Visitor (needs the exact tutorial-platform position), Fishtank (needs a full 7-location
Hazer spawn map). **Still not built or tested in-game** -- no .NET toolchain in this container.
Every vanilla member used (`SlugNPCAI.GetFoodType`, `SeedCob`, `PlayerSessionRecord.AddEat`/
`.playerNumber`, `CreatureTemplate.Type.SmallCentipede`/`CicadaA`/`CicadaB`) was verified against
`reference/GameSource/*.cs` first. Brace/paren balance re-checked across the whole `src/Primal`
tree after this change.

## Status update, 2026-09-16 (later still): the 6-challenge accounting gap -- Catalogue and Ascension implemented; Vegan, Friend, Saint, Scholar re-examined

The user caught a real gap: 36 implemented + 8 explicitly flagged as difficult (Journey, Visitor,
Fishtank, Outlaw, Triplopia, Broadcast, Heist, Elder) only accounts for 44 of 50. The missing 6 --
Catalogue, Vegan, Friend, Scholar, Saint, Ascension -- had simply never been revisited since the
original ratings pass. Checked each against what's actually buildable now:

- **Catalogue -- IMPLEMENTED.** "Place 15 unique objects/creatures into a shelter in one cycle."
  A direct, overdue consumer of `SurvivorShelterContents.CountDistinct` -- shared mechanism #2,
  built in the very first batch, sitting unused for creature/item dedup until now (Mother already
  used its sibling `CountObjectsOfType`, but nothing had used `CountDistinct` itself). Dedupes by
  a combined creature-or-item type key. **Known simplification:** the challenge's own rule is
  "every unique ICON is a unique object" -- this dedupes by raw type instead, since this mod has
  no icon-grouping data; could diverge if vanilla's own icon system groups some types together.
- **Ascension -- IMPLEMENTED.** Reuses the same ending hook Perseverance already consumes.
  Confirmed `RainWorldGame.clock` accumulates for the whole game session with no reset anywhere in
  `RainWorldGame.cs`/`SaveState.cs` -- exactly "IGT from game start," no extra timer needed.
  **Known simplification:** the CSV's own notes say the real measurement is "the Remix speedrun
  timer's value when it stops" -- this uses the mod's own raw `game.clock` instead, which could
  diverge from that real timer.
- **Vegan** -- re-examined, genuinely belongs alongside Visitor: both need exact room/position
  data inside the tutorial area (7 fruit locations + the charge-hop gap threshold) that hasn't
  been supplied. Same category of gap, not a new one.
- **Friend** -- re-examined, this is a REAL, distinct architectural gap that was correctly
  identified in the original ratings pass but never followed up on: "tamed" detection itself is
  solved (the same `friendTracker.friend` check `SleepoverTracker` already uses), but the
  challenge needs the RESULT tracked as a set SHARED ACROSS EVERY PRIMAL CHARACTER on the save
  slot ("Friends can be tamed in any amount of campaigns"), not per-character the way
  `PrimalSaveMarkers`/`DeathPersistentSaveData` are scoped. This mod's only existing
  cross-character persistence is the Remix-config-backed `Configurable&lt;T&gt;` layer
  `GimmickAssignments` already uses -- reusing that for a hidden, non-UI "which Lizard types has
  ANY character tamed" bitmask is plausible but genuinely new infrastructure, not investigated
  deeply enough this pass to build with confidence.
- **Scholar** -- re-examined. The enumeration mechanism is trivial now that Collector proved it
  out (`DataPearl.AbstractDataPearl.DataPearlType` is also confirmed an ExtEnum, so the same
  `values.entries` trick applies with zero new research). The real open question is SCOPE, and
  it's sharper here than it was for Collector: some registered pearl colors are tied to unique,
  one-off STORY pearls that may never appear as a loose collectible at all, so treating "all
  registered types" as the threshold (Collector's own approach) risks shipping a tracker that can
  never actually complete. Needs the same kind of scope confirmation Collector got before
  building it the same way.
- **Saint** -- re-confirmed as genuinely underspecified rather than newly noticed: "complete all 3
  objectives without killing anything, or eating live prey," with exclusions ("no karma caching,"
  "no echo-negated kills") that were already flagged in the very first ratings pass as "may end up
  partially honor-system by necessity, not just by choice." Nothing new found to change that.

38 of 50 challenges now have a tracker registered. **Still not built or tested in-game** -- no
.NET toolchain in this container. Brace/paren balance re-checked across the whole `src/Primal`
tree after this change.
