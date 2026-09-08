# Phase 5 roadmap: Early Access candidate

Status: **focused roadmap, active.** This file expands Phase 5 of `docs/design.md` section 7: PR-51 to PR-55. It applies D-1, D-15, D-96, D-112, D-142, D-152, D-170, D-180, and D-185. It does not restate a decision. It cites the D-# id. Written 2026-09-07 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the tenets (section 6.1). Phase 4 is `phase-4-content-complete.md`. Gate 4 must pass before PR-51 starts.

External facts: the Steam Deck verification requirements and the Apple notarization steps are external. Each PR that depends on one fetches it and records the date and the source (OQ-66, OQ-67).

Correction passes: 2026-09-07, the PR #1 review. Sequence step 11 is new. It makes the repository public, changes the mode file to `enforced`, and promotes `review-gate` to a required status check (D-170, D-180, D-185).

## 1. Thesis

Phase 5 turns the content-complete build into something a stranger can install and play on Steam, on all three platforms and on the Deck (D-1, D-15). It ends at Gate 5, the Early Access candidate. Nothing in this phase changes gameplay. The order puts the export pipeline first, because every later check runs on an exported build. Steamworks follows, then the settings screen, then the Deck pass. The crash report flow comes last, because it is the only item that can wait until the day before release without harm.

Two purchases gate this phase: the Apple Developer account before PR-51 and the Steam Direct fee before PR-52 (D-142, F-25).

## 2. Findings that bind this phase

| # | Finding | Binds |
|---|---|---|
| F-3 | The Steam Deck is the performance floor | PR-54 |
| F-23 | The name has no trademark search yet | The store page |
| F-25 | The Apple and Steam accounts do not exist | PR-51, PR-52 |
| F-44 | The Deck verification checklist is an external fact with no source or date in the plan | PR-54 |
| F-45 | D-152 changed the save files, and no item names which files cloud saves sync | PR-52 |

## 3. Guardrails for this phase

All guardrails in `docs/design.md` section 6.2 apply. These three matter most in Phase 5:

1. **G-13.** No attribution in a store page, a build, or a release note (T-6).
2. **G-15.** The Deck at 800p is the floor. PR-54 is the proof.
3. **G-16.** Steamworks is a dependency. PR-52 records its decision entry.

## 4. Roadmap

Each entry has: scope, out of scope, exit tests, review focus, the check clause, the gate, and a plain-English paragraph. The review skill is `.claude/skills/pr-review/SKILL.md`.

### PR-51: Export pipeline and notarization

Scope:

- `export_presets.cfg`: Windows x64, Linux x64, and a macOS universal binary, from the pinned Godot version (D-61). Credentials stay out of the repository (`.gitignore`).
- A CI job `export` that builds all three on a tag and uploads them as artifacts.
- Signature and notarization of the macOS build with the tool from OQ-67, run on the Mac Mini (D-142).
- Shader pre-warm at load: every material draws once behind a black frame before the hub appears.
- A clean-install test script per platform that installs the artifact on a fresh user account and runs the smoke session.

Out of scope: Steam upload (PR-52), auto-update.

Exit tests:

1. `ExportJobProducesThree` asserts three artifacts from one tag.
2. `MacBuildIsNotarized` asserts that the tool's own check finds the stapled notarization ticket.
3. `CleanInstallRunsSmoke` runs the smoke session from a clean install on each platform and asserts exit code 0.
4. `NoCredentialInRepository` asserts that git tracks no credential file.
5. `ShaderPrewarmCoversAllMaterials` asserts every material in the atlas set draws during the pre-warm.

Review focus: input and CI boundaries, presentation, dependencies.

Check clause: none.

Gate: exit tests 1 to 5 pass.

> *In plain English:* the game becomes something a player can install, also on a Mac that blocks unsigned software. The first launch never stutters on a new shader.

### PR-52: Steamworks

Scope:

- The Steamworks .NET dependency, with its decision entry (G-16).
- Achievements from the list in OQ-68, each bound to a Core event.
- Cloud saves for the file set of OQ-71: the profile and the suspended run record (D-96, D-152).
- The conflict rule per OQ-19: a prompt, never a silent choice.
- Steam upload configuration for the three depots.

Out of scope: leaderboards (D-96), Steam Input beyond the controller glyphs of PR-54.

Exit tests:

1. `AchievementFiresOnce` asserts one unlock per event and no repeat.
2. `CloudSyncsFileSet` asserts the OQ-71 files sync and nothing else.
3. `ConflictPrompts` creates two profiles with different generations and asserts a prompt with both summaries.
4. `NoSilentConflictResolution` asserts no code path resolves a conflict without the prompt (T-2).
5. `SteamAbsentIsHandled` runs the build without Steam and asserts a log line and a game that runs.

Review focus: persistence, errors, dependencies, input boundaries.

Check clause: none.

Gate: exit tests 1 to 5 pass.

> *In plain English:* your bank follows you between machines, and a clash between two copies asks you instead of a guess.

### PR-53: Settings and accessibility

Scope:

- `WhatYouCarry.Game/Ui/SettingsScreen.cs`: the settings from OQ-69, for a controller at Deck size (D-90, G-15).
- A rebind for keyboard, mouse, and controller, saved in the profile.
- Accessibility options per OQ-14, each with no effect on the simulation seed context unless D-53 changes.
- The string table complete for every screen (D-98).

Out of scope: localization (D-98).

Exit tests:

1. `EverySettingPersists` asserts each option survives a restart.
2. `RebindRoundTrips` asserts a rebind saves and loads.
3. `SettingsNavigate` asserts every control reachable with the controller model.
4. `StringTableComplete` asserts every screen's text ids exist in the table (G-8).
5. `AssistDoesNotEnterIntent` asserts an assist option changes no intent frame (D-77), unless OQ-14 decides otherwise.

Review focus: presentation, persistence, content.

Check clause: none.

Gate: exit tests 1 to 5 pass.

> *In plain English:* the options screen, complete text, and any assist options the owner chooses.

### PR-54: Steam Deck verification pass

Scope:

- Controller glyphs for the Deck's controls in every prompt.
- 800p text sizes on every screen, checked against the screenshot fixture of PR-19.
- Default settings that meet the M-3 target on the Deck (OQ-15).
- The Deck checklist from OQ-66, fetched and dated, with each item's evidence in this file.

Out of scope: any gameplay change.

Exit tests:

1. `GlyphsMatchController` asserts the Deck glyph set in every prompt when the Deck controller is active.
2. `TextMeetsDeckMinimum` asserts every label at or above the OQ-66 minimum size at 800p.
3. `DefaultsMeetFrameTarget` reruns M-3 on the exported build and asserts the target.
4. Every checklist item has evidence in this file, dated.

Review focus: presentation, dependencies and cost.

Check clause: none.

Gate: exit tests 1 to 4 pass.

> *In plain English:* a test confirms that the game plays well on the handheld.

### PR-55: Crash report flow

Scope:

- `WhatYouCarry.Game/Ui/CrashReportScreen.cs`: a screen after an assertion failure that cannot continue (D-112). It shows the report and offers to save the seed and the run record to the location of OQ-70.
- A copy-to-clipboard action for the report summary.
- No upload service in v1 (OQ-70).

Out of scope: telemetry, any network call.

Exit tests:

1. `ForcedAssertionShowsReport` triggers an unsafe assertion and asserts the screen with the seed.
2. `SavedReportReplays` saves the report and asserts a session can replay the run record to the failure tick.
3. `NoNetworkCall` asserts no network access in the report flow.
4. `ReportScreenNavigates` asserts the screen works with the controller model.

Review focus: errors, replay, presentation.

Check clause: none.

Gate: exit tests 1 to 4 pass.

> *In plain English:* when something goes wrong, the game hands the player a file that lets us replay the exact moment. It sends nothing anywhere on its own.

## 5. Sequence

One person owns the program. Items run one at a time in this order. Gate 4 must pass first.

1. Owner: buy the Apple Developer account (D-142). Answer OQ-17 and OQ-67.
2. PR-51.
3. Owner: pay the Steam Direct fee (D-142). Answer OQ-19, OQ-68, OQ-71.
4. PR-52.
5. Owner: answer OQ-14 and OQ-69.
6. PR-53.
7. Owner: answer OQ-66. Confirm OQ-15.
8. PR-54.
9. Owner: answer OQ-70.
10. PR-55.
11. Owner: make the repository public. Merge a PR that sets `.github/review-gate-mode` to `enforced` (D-185). Then set `review-gate` and the three-platform job as required status checks on `main` (D-170, D-180).
12. **← GATE 5 (Early Access candidate).** Every exit test in this file passes. The store page has no attribution (T-6). The owner signs the gate in `docs/decisions.md`.

## 6. Open questions

The register is `docs/questions.md` (D-144). These questions bind Phase 5. Each names the PR it blocks.

Open:

- OQ-14: accessibility. Blocks PR-53.
- OQ-15: the Deck frame target. Blocks PR-54.
- OQ-17: the trademark search. Blocks the store page.
- OQ-18: purchase dates. Blocks PR-51 and PR-52.
- OQ-19: cloud save conflicts. Blocks PR-52.
- OQ-66: the Deck checklist source. Blocks PR-54.
- OQ-67: the notarization tool. Blocks PR-51.
- OQ-68: the achievement list. Blocks PR-52.
- OQ-69: the settings list. Blocks PR-53.
- OQ-70: the crash report location. Blocks PR-55.
- OQ-71: the cloud save file set. Blocks PR-52.
