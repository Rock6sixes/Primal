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
| 19 | Friend | 4 | Needs the real "tamed" flag/threshold on `Lizard` (should exist, needs confirming) for each of ~9 species, tracked as a **save-file-wide** set rather than per-character -- the first challenge here that doesn't fit this mod's existing per-character marker scoping and needs a new save-scope path. |
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
| 30 | Sleepover | 3 | Same "specific live creature present at hibernation, not tamed, not killed" shape as Symbiote/Charged, with a Red-Lizard color check and a region check. |
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

**A cross-gimmick dependency this surfaced, worth remembering for any future kill-based
tracker:** Ranger and Spearfisher only ever see kills at all for a character that ALSO has the
separate "Kill List" gimmick assigned -- `CreatureKillListCycleWinHook` gates every kill it
commits on `GimmickAssignments.IsEnabled(characterID, CreatureKillListGimmick.ID)`, independent
of whether CommunityChallenges is assigned to that same character. Not a bug, just means both
gimmicks need assigning together for either of these two to ever complete.

**One deliberate simplification, flagged rather than silently assumed:** Ranger's "in one run" is
implemented against Kill List's own PERMANENT (whole-save-file) record, not anything scoped to a
single continuous life -- this mod has no existing "run boundary" concept to check against
(Kill List's own records don't reset on death either). Same spirit as this doc's own note on
Massacre/Outlaw using "our own tally, not vanilla's" -- revisit if a real run-boundary mechanism
gets built later.

Still entirely unbuilt/untested -- no .NET toolchain in this container.
