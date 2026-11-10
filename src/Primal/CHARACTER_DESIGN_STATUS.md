# Per-character gimmick/requirement design -- status audit (2026-09-18, updated same day/later rounds)

The user supplied a target design table (one gimmick + one requirement per row, grouped by
character) and asked which parts are already implemented before any new code got written. This
doc is that audit, verified against the actual source rather than assumed -- see each row for
exactly what was checked and how. Update it as the gaps below get closed instead of leaving it to
go stale (same policy AI_CONTEXT.md's own "Maintenance policy" section asks for) -- this revision
folds in every gap the original audit found being closed across several follow-up rounds the same
day.

## The target table, as given

| Character | Gimmick | Requirement |
| --- | --- | --- |
| Monk | Threat Panic | Ascend |
| Survivor | -- | Community Challenges: complete 45 of 50 (count configurable) |
| Hunter | -- | Reach 100k points / Ascend |
| Artificer | Chain creature on room entry (double check: only slugcat-enterable pipes/shortcuts get the visual chain circle) | Unique kill list |
| Gourmand | Misery | Food Quest (renamed from "Food Quest & Save") |
| Gourmand | Slugpup | Story ending |
| Spearmaster | Master Spear | Under 1 hour |
| Spearmaster | No Hold (only spears enabled) | Story ending |
| Rivulet | Precycle Timer (50% multiplier, 100% chance, 100% shelter failure) | Visit every shelter |
| Rivulet | Rain Timer (set to 0) | Story ending |
| Saint | Rain Timer (set to 0) | Story ending |
| Saint | Blizzard Timer (set to 60 minutes) | -- |
| Saint | No Hold (only enable itself after one cycle, everything checked) | -- |
| Inv | No Hold (Spears, Yeeks, Squidcadas) | "King" requirement (rename from "Highest Point"), 4 checkboxes: highest spot, cloak, slugpup, vulture mask |
| Inv | No Maul | -- |

## Gimmicks

| Character | Gimmick | Status |
| --- | --- | --- |
| Monk | Threat Panic | **Done.** Implemented (`ThreatPanicGimmick.cs`), defaulted to Monk in `GimmickDefaults.cs`. |
| Artificer | Chains | **Done, double-check passed.** No change needed (see prior audit round for the full trace). |
| Gourmand | Misery, Slugpup | **Done.** Both implemented, both defaulted to Gourmand. |
| Spearmaster | Master Spear (OneSpear) | **Done.** Implemented, defaulted to Spearmaster. |
| Spearmaster | No Hold (only spears enabled) | **Done.** `NoHoldSettings.PerCharacterDefaultBlocked` now has an explicit Spearmaster entry (Spears only) -- no longer a coincidence of the generic fallback, a real per-character default. |
| Rivulet | Precycle Timer | **Reworked 2026-11-10, compiles, not yet tested in-game.** Defaulted onto Rivulet. Head start set in seconds (0-480, default 60 = 1 min; Random rolls base..2.5x; Force max = 2.5x), chance default 100%, flood default 100%. "Force shelter unavailable" left off (Permanent Shelter Failure covers it). |
| Rivulet | Permanent Shelter Failure | **Built 2026-11-05, compiles, not yet tested in-game.** New `PermanentShelterFailureGimmick.cs`: every shelter you successfully sleep in stays broken (vanilla `World.brokenShelters`) for the rest of the save. Defaulted onto Rivulet. |
| Rivulet | Rain Timer (set to 0) | **Done.** Defaulted onto Rivulet with `RainTimerSettings.PerCharacterDefaultPercent` at 0%, same "starts essentially immediately" treatment as Saint. |
| Saint | Rain Timer (0), Blizzard Timer (60 min) | **Done.** Both implemented and defaulted to Saint with the exact requested values. |
| Saint | No Hold (only enable itself after one cycle, everything checked) | **Done.** "Everything checked" is `NoHoldSettings.PerCharacterDefaultBlocked`'s own Saint entry (all ten categories + Other), defaulted onto Saint in `GimmickDefaults.cs`. "Inert until cycle N" is a new, genuinely general mechanic on `NoHoldGimmick` itself (`NoHoldSettings.GetInertUntilCycle`, a per-character textbox, 0 = active immediately) -- Saint's own default is now 1 (2026-11-10, user: skip the first cycle), everyone else 0; settable per-character via Remix. |
| Inv | No Hold (Spears, Yeeks, Squidcadas) | **Done.** `NoHoldSettings.PerCharacterDefaultBlocked` has an Inv (Sofanthiel) entry for exactly these three categories, defaulted onto Inv. |
| Inv | No Maul | **Done.** Implemented, defaulted to Inv. |

## Requirements

| Character | Requirement | Status |
| --- | --- | --- |
| Monk | Ascend | **Done.** Resolved by the new "Ending" requirement (renamed from "Reach Ending"), defaulted onto Monk with its own select defaulting to "Ascension". |
| Survivor | Community Challenges (complete N of 50, default 45) | **Built 2026-11-10, compiles, not yet tested in-game.** New `CommunityChallengeCountRequirement` ("Community Challenges", setting 1-50, default 45), Survivor's only default requirement; Survivor has no default gimmicks. Tracks all 50 automatically (`GimmickAssignments.IsTracked`). Outlaw is no longer Hunter-only, so all 50 are completable by any character. |
| Hunter | Reach 100k points | **Done.** `OutlawRequirement`'s threshold is now a per-character `Configurable<int>` textbox (`OutlawSettings`), defaulting to 100,000, defaulted onto Hunter. |
| Hunter | Ascend | **Done.** Same "Ending" requirement as Monk's row, also defaulted onto Hunter with "Ascension". |
| Artificer | Unique kill list | **Done.** `UniqueKillsRequirement`, fully general (no character lock), assignable to Artificer. Not defaulted onto Artificer specifically this round -- not part of this round's explicit requests (see AI_CONTEXT.md's own note on this same omission). |
| Gourmand | Food Quest | **Done.** `GourmandFoodQuestRequirement.DisplayName` renamed from "Food Quest & Save" to "Food Quest", defaulted onto Gourmand. |
| Gourmand | Story ending | **Done.** Resolved by "Ending", defaulted onto Gourmand with its own select defaulting to "Story" (the fallback). |
| Spearmaster | Under 1 hour | **Done.** `SpearmasterFastEndingRequirement` ("Quick Broadcast"), exactly 60 minutes, already matches. Not defaulted onto Spearmaster specifically this round -- not part of this round's explicit requests. |
| Spearmaster | Story ending | **Done.** Resolved by "Ending", defaulted onto Spearmaster with "Story". |
| Rivulet | Sleep in every shelter | **Built 2026-11-05, compiles, not yet tested in-game.** New `SleepEveryShelterRequirement` ("Sleep In Every Shelter"): only a successful hibernation counts, not a visit. Replaces Visit Shelter as Rivulet's default (Visit Shelter stays available by hand). Skips shelters the timeline permanently breaks. |
| Rivulet | Visit every shelter | **Done.** New `VisitShelterRequirement` ("Visit Shelter"), defaulted onto Rivulet -- see its own doc comment in `Requirements.cs` for the full trace (`World.shelters` + `RegionState.roomsVisited` + `SlugcatStats.SlugcatStoryRegions`, checked across every one of this character's own story regions, not just one). Scope was resolved as "every shelter in every story region" (the more literal reading of the request), not "one region" -- flagged as the more ambitious of the two readings the original audit left open, in case a narrower scope was actually wanted. |
| Rivulet, Saint | Story ending | **Done.** Resolved by "Ending", defaulted onto both with "Story". |
| Inv | "King" requirement, 4 checkboxes | **Done.** `InvHighestPointRequirement.DisplayName` renamed from "Highest Point" to "King"; all four conditions (highest screen, cloak, slugpup, vulture mask) are now independent per-character `Configurable<bool>` checkboxes (`InvHighestPointSettings`), all defaulting to checked (reproducing the old hardcoded "all four required" behavior). Defaulted onto Inv. |

## Architecture added to close the two original gaps

1. **Settings/config UI on `Requirement`** -- `BindSettings`/`BuildSettingsUI`/`SettingsContentHeight`/
   `HasSettings`, mirroring `GimmickDef` exactly. First used by "Ending"'s Story/Ascension/Either
   select, now also used by Outlaw's score textbox and King's four checkboxes.
2. **Default-assignment mechanism for Requirements** -- `RequirementDefaults.cs`, the Requirements-side
   counterpart to `GimmickDefaults.cs`, calling the same `GimmickAssignments.SetDefault` (genuinely
   generic over gimmick and requirement IDs alike) with requirement IDs instead. Wired into
   `RainWorldHooks.Postfix` right after `GimmickDefaults.Apply()`.

## Resolved this round (previously "still needing the user's own decision")

- **Rivulet's Precycle Timer "50% multiplier"** -- resolved: Precycle Timer's duration is now a
  percentage multiplier on vanilla's own roll bounds. No specific percent was requested for
  Rivulet's own default, so it isn't assigned/tuned yet -- fully configurable by hand.
- **Rivulet's "Visit every shelter"** -- resolved as "every shelter in every story region" (see
  `VisitShelterRequirement`'s own doc comment for the full reasoning and the alternate, smaller
  change if a per-region-only scope turns out to be what was actually wanted).
- **Saint's "No Hold... only enable itself after one cycle"** -- resolved as a new, fully general
  per-character "inert until cycle N" setting on `NoHoldGimmick` itself (reads
  `SaveState.cycleNumber`, 0-indexed). Saint's own specific cycle count was not requested, so it's
  left at the generic default (0, no delay) rather than guessed.

## Deliberately not defaulted this round (flagging, not assuming)

`SpearmasterFastEndingRequirement` -> Spearmaster and `UniqueKillsRequirement` -> Artificer both
appear in the original design table's own Requirement column, but neither was explicitly
re-requested in the message that asked for every other default-assignment in this document --
both remain fully assignable by hand via Remix in the meantime. Same reasoning as
`RequirementDefaults.cs`'s own header comment.
