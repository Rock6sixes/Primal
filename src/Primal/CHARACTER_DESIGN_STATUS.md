# Per-character gimmick/requirement design -- status audit (2026-09-18)

The user supplied a target design table (one gimmick + one requirement per row, grouped by
character) and asked which parts are already implemented before any new code got written. This
doc is that audit, verified against the actual source rather than assumed -- see each row for
exactly what was checked and how. Nothing in this doc was changed as part of writing it; it's a
snapshot to work from, not a implementation log. Update it as the gaps below get closed instead of
leaving it to go stale (same policy AI_CONTEXT.md's own "Maintenance policy" section asks for).

## The target table, as given

| Character | Gimmick | Requirement |
| --- | --- | --- |
| Monk | Threat Panic | Ascend |
| Survivor | -- | All 50 community challenges |
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
| Artificer | Chains | **Done, double-check passed.** `ChainsGimmick.cs`'s `IsSealableExit` (RoomExit/RegionTransportation/CreatureHole) decides what gets mechanically sealed -- has to include CreatureHole so creatures can't flee via their own dens. `IsPlayerPipe` (RoomExit/RegionTransportation only, explicitly excluding CreatureHole) is the SEPARATE, narrower set that actually gets passed to `MultiChain`/`AssignNewChain` for the visual chain anchors. Confirmed by reading both call sites directly: den entrances are sealed but never get a chain circle. No change needed. |
| Gourmand | Misery, Slugpup | **Done.** Both implemented, both defaulted to Gourmand. |
| Spearmaster | Master Spear (OneSpear) | **Done.** Implemented, defaulted to Spearmaster. |
| Spearmaster | No Hold (only spears enabled) | **Done, but see the per-character-default note below.** No Hold is assigned to Spearmaster by default and its GLOBAL default (Spears blocked, nothing else) happens to already match what's wanted here -- it isn't a per-character override, just a coincidence of the current global default. |
| Rivulet | Precycle Timer (50% multiplier, 100% chance, 100% shelter failure) | **Not assigned to Rivulet at all.** Also: **"50% multiplier" doesn't map to anything that currently exists** -- Precycle Timer's duration is a fixed number of minutes, not a percent-of-normal multiplier the way Rain Timer's own timer is. Needs clarification before this can be built as specified. |
| Rivulet | Rain Timer (set to 0) | **Not assigned to Rivulet** (only Saint has it right now). Gimmick itself is fully implemented; this is purely a missing default assignment + per-character default value (0%), same pattern already used for Saint. |
| Saint | Rain Timer (0), Blizzard Timer (60 min) | **Done.** Both implemented and defaulted to Saint with the exact requested values. |
| Saint | No Hold (only enable itself after one cycle, everything checked) | **Two gaps.** (1) "everything checked" needs a per-character default category set (see the architecture note below -- NOT a missing capability, just a missing default table). (2) "only enable itself after one cycle" is a genuinely new mechanic -- no gimmick in this codebase currently has any notion of "inert until cycle N of the save." Needs new code. |
| Inv | No Hold (Spears, Yeeks, Squidcadas) | Same per-character-default gap as Saint's row -- Inv currently gets only "Spears" blocked by default. |
| Inv | No Maul | **Done.** Implemented, defaulted to Inv. |

### The per-character-default question, resolved

The user pointed out (correctly) that gimmick *settings* are already stored per-character --
confirmed by re-reading `NoHoldGimmick.cs`: every category's `Configurable<bool>` is bound under a
key of `characterID + "_NoHold_" + category.Key`, so Saint and Inv already get their own
independent, separately-saved checkboxes per category. **The architecture was never the
limitation.** The actual gap is narrower: the *default value* fed into `Bind()` for each category
comes from one global `Categories[].DefaultBlocked` array, the same for every character, instead
of a per-character lookup -- exactly the situation `RainTimerSettings`/`BlizzardTimerSettings`
already solved for Saint's own defaults (`characterID == Saint.value ? SaintDefault : Default`).
Fix: give `NoHoldSettings.Bind()` (and any future per-character-default gimmick) a per-character
default-category table instead of the single global array, same pattern already proven twice.

## Requirements

| Character | Requirement | Status |
| --- | --- | --- |
| Monk | Ascend | **Doesn't exist as asked.** The only ascension-tracking requirement is `AscensionRequirement`, a 38-minute SPEEDRUN challenge (one of the 50 curated Community Challenges), not a bare "did you reach the ending." Needs either reusing that (accepting the time limit) or a new untimed requirement -- see the new "Reach Ending" requirement below, which resolves this. |
| Survivor | All 50 community challenges | **Not defaulted to anyone**, and there's no single gimmick left to assign (`CommunityChallengesGimmick.cs`'s own header confirms this: "There is no 'Community Challenges' gimmick to assign anymore"). Each of the 50 is its own Requirement ID that would need individually default-assigning to Survivor. Also: **there is no default-assignment mechanism for Requirements at all** -- `GimmickDefaults.cs` has no Requirements equivalent; every requirement defaults to off for every character until a player manually enables it in Remix. |
| Hunter | Reach 100k points | Exists as `OutlawRequirement`, but the threshold is hardcoded to **10,000, not 100,000**. One-line fix once touched. |
| Hunter | Ascend | Same gap as Monk's row. |
| Artificer | Unique kill list | **Done.** `UniqueKillsRequirement`, fully general (no character lock), assignable to Artificer. |
| Gourmand | Food Quest | Exists as `GourmandFoodQuestRequirement`, `DisplayName` currently "Food Quest & Save" -- needs renaming to just "Food Quest". |
| Gourmand | Story ending | Doesn't exist as a standalone requirement -- resolved by the new "Reach Ending" requirement below. |
| Spearmaster | Under 1 hour | **Done.** `SpearmasterFastEndingRequirement` ("Quick Broadcast"), exactly 60 minutes, already matches. |
| Spearmaster | Story ending | Same gap as Gourmand's -- resolved by "Reach Ending". |
| Rivulet | Visit every shelter | **Doesn't exist anywhere** -- confirmed via a full grep of `Requirements.cs`. Needs new code (would need `SurvivorShelterContents` or similar to enumerate/track every shelter in a region or the whole game -- scope needs deciding). |
| Rivulet, Saint | Story ending | Same gap, resolved by "Reach Ending". |
| Inv | "King" requirement, 4 checkboxes | Exists as `InvHighestPointRequirement` ("Highest Point"), but **all 4 conditions (highest screen, slugpup, mask, cloak) are hardcoded as simultaneously required** -- no per-condition toggle exists. This is the bigger of the two gaps here: `Requirement` (the base class) has **no settings/UI mechanism at all**, unlike `GimmickDef` (which has `BindSettings`/`BuildSettingsUI`) -- adding checkboxes here means extending the base class first, which is now in progress (see "Requirements config menus" below). |

## Two architectural gaps identified, both now being addressed

1. ~~No per-character default *values* for a gimmick's own settings~~ -- **not actually a gap**,
   see "The per-character-default question, resolved" above. Just needs a per-character default
   table, the same pattern already used twice.
2. **No settings/config UI on `Requirement` at all**, and **no default-assignment mechanism for
   Requirements**. Both needed for Inv's King checkboxes, Survivor's 50 defaults, and every "assign
   this by default" requirement row above. Work started 2026-09-18 (later) -- see AI_CONTEXT.md's
   dated entries from that point on for progress.
3. A **generic "reached a standard story ending" requirement** doesn't exist -- referenced by name
   four times in the table (Gourmand, Spearmaster, Rivulet, Saint) but only ever implemented today
   as one-off, character-specific, *timed* variants (Ascension/SpearmasterFastEnding). Being added
   as a new general "Reach Ending" requirement with a per-character Story/Ascension select, which
   also resolves the "Ascend" rows for Monk and Hunter (pick "Ascension" there) without needing a
   second new requirement.

## Still needing the user's own decision (not resolved by this doc alone)

- **Rivulet's Precycle Timer "50% multiplier"** -- Precycle Timer's duration is currently an
  absolute minutes value, not a percent-of-normal multiplier. Does this row want a new
  multiplier-style duration control (mirroring Rain Timer's own design), or does "50%" describe
  something else entirely?
- **Rivulet's "Visit every shelter"** -- scope: every shelter in one region, or literally every
  shelter in the whole game? Either is a real, currently-unbuilt tracking feature.
- **Saint's "No Hold... only enable itself after one cycle"** -- a new delayed-activation concept
  with no precedent in this codebase yet. Needs a design pass of its own before implementation
  (is "one cycle" measured from save start, from when the gimmick was assigned, or something else
  if the player switches characters?).
