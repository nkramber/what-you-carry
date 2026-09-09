# Phase 1 roadmap: Foundations

Status: **focused roadmap, active.** This file expands Phase 1 of `docs/design.md` section 7: PR-1 to PR-11, PR-58, PR-59, M-1, and M-2. It applies D-148 to D-152, D-156, D-157, D-159 to D-168, D-170, D-171, D-173, and D-175. It also applies D-176 to D-178, D-180, D-182 to D-185, D-189, D-190, D-194, D-196 to D-199, and D-200 to D-264. It does not restate a decision. It cites the D-# id. Written 2026-09-07 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the tenets (section 6.1). This file adds per-PR scope, exit tests, review focus, and the questions that each PR needs answered before it starts.

External facts: none new. The Godot version is in the design header, verified 2026-09-07.

Correction passes: 2026-09-07, the PR #1 review. D-176 to D-178 correct the attribution reading, the night gate bootstrap, and six superseded references. D-179 to D-181 add the `review-gate` job to PR-1. D-181 revises D-179, D-184 revises the effective head, and D-185 revises the mode source. PR-58 is new, and it holds the night gate. D-189 puts the SDK on the CI runner through `actions/setup-dotnet`. D-190 adds the override label to the `review-gate` job. D-194 sets the solution format after a smoke run on the runner. 2026-09-07, PR-1: a run of the gate against `main` refuted the claim that PR-1 passes its own check with no owner action. D-196 puts the mode file on `main` first. 2026-09-08, the PR #6 review. D-197 moves the gate to `pull_request_target`, and D-198 accepts the review-record risk with a named commit in the output. 2026-09-08, PR-2: the first run of the checker found 77 findings in 15 files, and the PR rewrote each sentence. 2026-09-08, PR-3: a measurement refuted D-161, and D-203 folds to [-pi/4, pi/4] instead (F-60). D-200 to D-202 answer the `Pow` contract, the location of the bit-identity program, and the compiler API dependency. The review took seven passes on the lint boundary. D-204 to D-208 record the answers. D-204 stops conditional compilation in Core. The boundary then moved from a namespace list to a type list, and to a member list with the overload arity (F-65 to F-69). D-209 gives a review its scope, after the same finding reopened four times (F-70). 2026-09-08, PR-4: five owner questions came before the code, and D-211 to D-216 record the answers. The PR #15 review took three passes on the logger, and F-72 to F-77 record the findings. D-217 lets a review correct a stale fact in the PR description directly. D-218 ratifies the log name and value rule that finding P2-7 produced. 2026-09-08, PR-5: four owner questions came before the code, and D-219 to D-223 record the answers. D-219 applies D-211 to the content bytes, and the review found the hash framing and the `Get` rule that F-78 and F-79 record. 2026-09-09, PR-6: seven owner questions came before the code, and D-224 to D-230 record the answers. D-226 corrects the frame wording of the design doc (F-80), and D-227 sets the Phase 1 loop state. The reader of D-220 gains an empty list and a null (D-229), and F-81 records an enum inside an interpolation. 2026-09-09, the PR-6 merge: four owner questions came before the PR-7 code. D-231 to D-234 record the motion constants, the button bits, the movement mapping, and the coordinate frame. 2026-09-09, PR-7: six more owner questions came before the code, and D-235 to D-240 record the answers. A float check refuted an exact contact on a block face (F-82), and the sweep stops one skin before it (D-235). The fixed-step apex is 1.17 meters and not the 1.23 meters of the closed form (F-83). 2026-09-09, the PR-7 merge: seven owner questions came before the PR-8 code. D-241 to D-247 record the pitch limit, the camera geometry, the controller aim flag, and the assist shape. They also record the camera state, the boom sweep, and the aim ray origin. D-241 revises the pitch limit of D-227, and D-243 assigns bit 8 of D-232. 2026-09-09, PR-8: two more owner questions came before the code, and D-248 and D-249 record the pitch sign and the shoulder march. 2026-09-09, the PR #23 review: the bit-identity sweep folded the camera from a second live loop, and the replay gained an observer (F-85). 2026-09-09, the PR-8 merge: six owner questions came before the PR-9 code, and D-252 to D-257 record the answers. The owner asked for a floor far more random, varied, and detailed than rectangles, and D-253 sets the mine dig plan. D-254 splits the work into PR-9 and a new PR-59, and D-257 assigns bit 9 of D-232. 2026-09-09, the PR-9 merge: six owner questions came before the PR-59 code, and D-258 to D-263 record the answers. Still water is passable and one block deep, and a body walks and jumps through it at half pace. D-259 numbers the block ids, and D-260 raises the simulation version for a layout change. The automated pass of PR #28 refuted the first text of D-262, and F-86 records the correction: the velocity halves and gravity quarters. 2026-09-09, PR-59: the automated pass of PR #29 asked for a decision on the water probe, and D-264 records it. 2026-09-08, PR-3: a measurement refuted D-161. Degree 7 on [-pi, pi] reaches 2.5e-4 for sine, and D-203 folds to [-pi/4, pi/4] instead (F-60). D-200 gives `Pow` an integer exponent, D-201 puts the bit-identity program in `WhatYouCarry.Tools`, and D-202 records the compiler API dependency. Exit test 5 asked for the opposite of D-160, and this pass corrects it (F-61).

## 1. Thesis

Phase 1 builds the part of the game that no player sees and every later PR stands on. It ends at Gate 1, a foundation gate with no playtest (D-150). The order inside the phase follows dependency. The scaffold and the STE checker come first, because every later PR must pass the gate they create (D-148). DetMath, the RNG, and the bit-identity job come next, because no simulation math can exist before them (D-69). The loop, the record, the world, the camera, procgen, and projectiles follow in the order that each one reads the one before. The bot harness comes last, because it needs a run to run (D-149).

Each PR below lists its exit tests. An exit test is a named test or check that must pass before the PR can merge. The list is the written exit test that D-150 requires for Gate 1.

## 2. Findings that bind this phase

The design register in `docs/design.md` section 5 holds every finding. These rows bind Phase 1.

| # | Finding | Binds |
|---|---|---|
| F-13 | Windows x64 was absent from the determinism matrix | PR-1, PR-3 |
| F-15 | The harness default adds a co-author trailer | Every PR |
| F-25 | The SSD arrives 2026-09-08 | PR-1 |
| F-28 | The gate required checks before they existed | PR-1, PR-2, PR-3 |
| F-29 | Four gates preceded their prerequisites | PR-7, PR-9, PR-11 |
| F-30 | The run record omitted the initial state and versions | PR-6 |
| F-38 | PR-10's gate named a weapon roster that does not exist until Phase 3 | PR-10 |
| F-39 | The macOS CI leg needs a self-hosted runner that nobody has registered. D-157 names the setup | PR-1 |

## 3. Guardrails for this phase

All guardrails in `docs/design.md` section 6.2 apply. These three matter most in Phase 1:

1. **G-1.** Core has no engine dependency. PR-1 adds the test that asserts it.
2. **G-2 and G-21.** No `System.Math`, `MathF` outside DetMath, SIMD, reflection, `System.Random`, or wall-clock reads in Core. PR-3 adds the lint tool that enforces it.
3. **G-19.** A PR that creates a check passes that check. PR-1 to PR-3 name the absent checks (D-148).

## 4. Roadmap

Each entry has: scope, out of scope, exit tests, review focus, the check clause, the gate, and a plain-English paragraph. Review focus lists the rows of the review contract that apply. The review skill is `.claude/skills/pr-review/SKILL.md`.

### PR-1: Repository scaffold

Status: merged 2026-09-08 as PR #6, commit `a3b20e2`.

Scope:

- `WhatYouCarry.slnx` with four projects (D-108, D-66, D-194). The .NET 10 SDK creates that XML format by default, and Godot 4.7.2 accepts it (F-55):
  - Core: a class library with no package or project references.
  - Game: a Godot .NET project that references Core.
  - Tools: a console project.
  - Tests: an xUnit project that references Core and Tools.
- `global.json` that pins the .NET 10 SDK (D-62, D-173). `Directory.Build.props` with nullable on, warnings as errors, and one language version (D-68).
- `project.godot` and the Game project file on `Godot.NET.Sdk` at the pinned version (D-61).
- `.github/workflows/ci.yml` with three jobs: build and test on hosted Linux x64, hosted Windows x64, and the self-hosted macOS arm64 runner (D-100, D-148, OQ-31).
- `.github/pull_request_template.md` with the gate checklist and one "no change needed because" line per document (D-118). It has one line that names each absent check with the PR that creates it (D-148).
- `.github/workflows/review-gate.yml` on the `pull_request_target` event (D-179, D-181, D-185, D-197). GitHub runs the workflow file and the tool from the base branch, and the job fetches the PR head as data only. GitHub triggers the event only when the file exists on the default branch (F-58), so the check first runs on the PR after PR-1. The event types are `opened`, `reopened`, `synchronize`, `labeled`, and `unlabeled`. The two label types make the check run again when the owner adds or removes the label (D-190). It needs `checks: write`, `contents: read`, and `pull-requests: read`.
- The job first reads the labels of the PR. The label `review-override` selects the override path of D-190, and the job then applies the override rules below.
- Without that label, the job reads the PR number and applies three rules:
  - `docs/reviews/pr-<number>.md` exists on the PR head.
  - The verdict in that file is `Ready for owner merge`, and not `Blocked` or `Changes required`.
  - The head that the file records is the effective head. The effective head is the newest commit that changes a path outside the metadata set (D-184). A short hash matches by prefix.
- The metadata set is `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md` (D-184).
- The override rules of D-190 are:
  - Every changed path is in the eligible set. The eligible set is `docs/`, `CLAUDE.md`, `AGENTS.md`, and `.claude/skills/`.
  - No commit outside the metadata set is newer than the `labeled` event for `review-override`. Read that time from the PR timeline.
- The workflow computes the changed paths from the diff of the base branch and the PR head. A PR cannot claim an eligibility that it does not hold.
- One path in the code set fails the override, and the message names that path (T-2). The code set is every path outside the eligible set.
- The label overrides a review file that does not approve. The owner is the only account that merges (D-126).
- Compute the effective head with one `git log` command that excludes each metadata path by pathspec. Do not use a pipeline that stops early, because its exit status is not reliable.
- The job publishes a check run named `review-gate` on the PR head commit, through the Checks API (D-181, D-185). A workflow job cannot set a neutral conclusion by its exit code. The job fails on a `neutral` conclusion too, so the visible job line reads red until a record or a valid label exists (D-251).
- The conclusions are:

| Condition | `advisory` mode | `enforced` mode | Shows as |
|---|---|---|---|
| No review file for the PR number | `neutral` | `failure` | Grey check run and a red job (D-251), then red at launch |
| Verdict is not `Ready for owner merge` | `failure` | `failure` | Red |
| The recorded head is not the effective head | `failure` | `failure` | Red |
| Approved review of the effective head | `success` | `success` | Green |
| The `review-override` label, and no code path changed | `success` | `success` | Green |
| The `review-override` label, and one code path changed | `failure` | `failure` | Red |
| The `review-override` label, and a newer commit outside the metadata set | `failure` | `failure` | Red |

- `.github/review-gate-mode`: a tracked file that holds `advisory` or `enforced` (D-185). PR-1 creates it with `advisory`. The workflow reads the base branch, so the owner put the same file on `main` before the merge (D-196). PR-1 then passes its own check (G-19).
- The workflow reads the mode file from the base branch, never from the PR head (D-185). A PR must not change the mode that judges it.
- An absent file, an empty file, or an unknown value fails the job and names the file (T-2). Do not default the value.
- Neutral is correct only while the check is advisory. GitHub counts a neutral conclusion as a success for a required check (F-51).
- The check run output names the rule, the expected value, and the value it found (T-2, D-113).
- A build and test command section in `CLAUDE.md` and `AGENTS.md`, identical (D-122).

Out of scope: any Core type beyond an empty namespace, any scene, any content file.

Exit tests:

1. `dotnet build` succeeds on all three platforms in CI.
2. `AgentFilesAreIdentical` reads both agent files and asserts equal bytes (D-122).
3. `CoreReferencesNoEngine` reads the Core project file and asserts no package reference and no project reference (G-1).
4. The CI workflow has one job per platform, and the macOS job selects the self-hosted runner label.
5. The PR description names the STE checker, the lint tool, and the bit-identity job as absent, with PR-2 and PR-3 (D-148).
6. No commit subject or body in the PR names an agent, harness, or model as the source of the work (T-6, D-175, D-176). The scan covers co-author trailers and generation lines.
7. `ReviewGateIsNeutralWithNoReviewFile` asserts a neutral conclusion in `advisory` mode when no review file exists.
8. `ReviewGateFailsOnMissingFileWhenEnforced` asserts a failure conclusion in `enforced` mode for the same input.
9. `ReviewGateFailsOnChangesRequired` asserts a failure on a fixture review file with the verdict `Changes required`.
10. `ReviewGateFailsOnBlocked` asserts a failure on a fixture review file with the verdict `Blocked`.
11. `ReviewGateFailsOnStaleHead` asserts a failure when the recorded head precedes a later commit outside the metadata set.
12. `ReviewGateIgnoresMetadataCommit` asserts a success when the only later commit changes the metadata paths alone (D-184).
13. `ReviewGateFailsOnHandoffPlusCodeCommit` asserts a failure when a later commit changes a metadata path and any other path.
14. `ReviewGatePassesOnApproval` asserts a success on the verdict `Ready for owner merge` at the effective head.
15. `ReviewGateFailsOnUnsetMode` asserts a failure when the mode file is absent, empty, or unknown, and asserts that the message names the file (T-2).
16. `ReviewGateReadsModeFromBase` asserts that a PR which changes `.github/review-gate-mode` does not change its own mode (D-185).
17. Each failure output names the rule, the expected value, and the value found (T-2).
18. The `review-gate` check run appears on the PR head commit (D-181, D-185).
19. `ReviewGatePassesOnOverrideLabel` asserts a success when the label is present and every changed path is in the eligible set (D-190).
20. `ReviewGateFailsOnOverrideLabelWithCodePath` asserts a failure when the label is present and one changed path is in the code set. The message names that path (D-190, T-2).
21. `ReviewGateFailsOnOverrideLabelBeforeNewCommit` asserts a failure when a commit outside the metadata set is newer than the label event (D-190).
22. `ReviewGateRunsOnLabelEvent` asserts that the `labeled` and `unlabeled` event types start the workflow (D-190).
23. `SolutionIsSlnx` asserts that `WhatYouCarry.slnx` exists, that the repository holds no `.sln` file, and that the solution lists the four projects (D-194).
24. `ReviewGateRunsOnPullRequestTarget` asserts the `pull_request_target` event, and that no step checks out the PR head (D-197).
25. `ReviewGateNamesTheCommitThatChangedTheReviewFile` asserts that the output names the commit that last changed the review file, with its subject (D-198).
26. `ReviewGateJobFailsOnNeutral` asserts that the last step of the workflow exits 1 on a neutral conclusion, added 2026-09-09 (D-251).

Review focus: Core boundary, input and CI boundaries, dependencies, documents.

Check clause: the STE checker, the lint tool, and the bit-identity job do not exist. PR-2 and PR-3 create them. Branch protection does not exist, so `review-gate` is advisory until launch (D-170, D-180). The `review-gate` check does not run on PR-1 itself (D-197, F-58). A throwaway PR against `main` after the merge proves it.

Gate: exit tests 1 to 26 pass.

> *In plain English:* this makes the empty project with its four parts. It adds the automatic build on three kinds of computer and the checklist every change must fill in. It also adds a check that turns red when a change has no approved review. It adds nothing that plays.

### PR-2: STE checker

Status: merged 2026-09-08 as PR #10, commit `d5eb298`.

Scope:

- `WhatYouCarry.Tools/SteCheck/`: a console command that reads each hand-written `.md` file and reports each sentence that breaks a rule (D-130, D-139).
- Rules:
  - the 20-word limit in a numbered step, and the 25-word limit elsewhere.
  - semicolons and contractions.
  - passive voice, found by an auxiliary plus a past participle.
  - helper verbs.
  - -ing forms at a sentence start or after a preposition.
- Word counts follow rules 8.5 to 8.7: parentheses, hyphenated words, numbers, and identifiers count as one word. Headings count as one word.
- A numbered item counts as a procedural step, with the 20-word limit, only inside a section whose heading contains "Sequence" or "Procedure". Every other numbered item uses the 25-word limit.
- Exempt by path: `docs/reviews/`, `docs/session-handoff.md`, `docs/session-handoff-archive.md`, `docs/archive/`. Exempt by block: tables and fenced code.
- Output: one line per finding with file, line, rule id, and the sentence. A non-zero exit code on any finding.
- A reference check that reads the `Effect` column of `docs/decisions.md` (D-178, D-186). It reports each file outside that register that cites a superseded decision as a current answer.
- The check keys on `Superseded by D-N` only. A decision marked `Revised in part by D-N` stays citable, because the part that a citation names can still be current (D-186).
- The check skips a line that holds `supersedes`, and a line that names the superseding decision beside the superseded one. Both forms are self-consistent.
- A session number check that fails on a duplicate `## Session <number>` heading in `docs/session-handoff.md` (D-187).
- A CI job `ste-check` that runs the command and the reference check on every non-exempt `.md` file.

Out of scope: the STE dictionary, spell checks, term consistency.

Exit tests:

1. One unit test per rule with a sentence that passes and a sentence that fails.
2. `RepositoryDocumentsPass` runs the checker on the repository and asserts zero findings.
3. `FixtureFindsEveryRule` runs the checker on a fixture file with one violation per rule and asserts one finding per rule.
4. The `ste-check` CI job exists and runs on the PR.
5. The PR description names the lint tool and the bit-identity job as absent, with PR-3 (D-148).
6. `RepositoryCitesNoSupersededDecision` runs the reference check on the repository and asserts zero findings.
7. `ReferenceCheckFindsAStaleCitation` runs the check on a fixture that cites a superseded decision and asserts one finding.
8. `ReferenceCheckAllowsAPartialRevision` runs the check on a fixture that cites a decision marked `Revised in part by` and asserts zero findings (D-186).
9. `SessionNumberCheckFindsADuplicate` runs the check on a fixture handoff with two entries of the same number and asserts one finding (D-187).

Review focus: errors, input boundaries, documents, test quality.

Check clause: the lint tool and the bit-identity job do not exist. PR-3 creates them. This PR passes its own checker (G-19).

Gate: exit tests 1 to 9 pass.

Result, 2026-09-08: every exit test passes. The reference check skips the exempt paths too, because a dated record is history, and a rewrite to name the reviser falsifies it. The checker treats a colon that a space follows as a sentence end in any text (8.4). Text in backticks, quotes, or parentheses is one opaque word, and parentheses can nest (8.5, 8.6). The `ste-writing` skill records the rules and the exemptions.

> *In plain English:* this adds a tool that reads every document and reports each sentence that breaks the text rules. Documents are the project's memory, so the tool guards that memory.

### PR-3: Seeded RNG, DetMath, lint, and the bit-identity CI job

Status: merged 2026-09-08 as PR #12, commit `9e6fd5f`.

Scope:

- `Core/Determinism/Rng.cs`: xoshiro128** streams seeded by SplitMix64 from the 64-bit run seed, one stream per subsystem (D-159).
- `Core/Determinism/DetMath.cs`: `Sin`, `Cos`, `Atan2`, `Sqrt`, `Pow`, `Abs`, `Floor`, `Clamp`, and `Lerp` in float (D-70). The range reduction and each polynomial use only add, subtract, multiply, divide, and the IEEE square root. `Sqrt` wraps the IEEE square root, which is a basic operation. The accuracy target is D-161. `Sin` and `Cos` fold to [-pi/4, pi/4] and a quadrant, and `Atan2` folds to the octant (D-203). The domain limit is 4096 radians. `Pow` takes an integer exponent (D-200).
- `Core/Determinism/StateHash.cs`: FNV-1a 64 over the raw bit patterns of a state, in a fixed declared field order (D-160).
- `WhatYouCarry.Tools/DetLint/`: the `det-lint` command. It compiles the Core sources with the C# compiler API and reports each banned symbol (D-67, D-202, G-2, G-21). It reads symbols and never source words, so a Core type can carry the name of a banned platform type. It also reports each `#if`, because Core holds no conditional compilation (D-204). Core approves every type outside this project by name, and every member of one by name and overload arity (D-207, D-208). No namespace passes as a whole, and no type approves its whole surface, because each one holds machine-dependent members beside the ones Core needs. The banned symbols are `System.Math`, `MathF` outside `DetMath.cs`, `System.Numerics.Vector`, `System.Runtime.Intrinsics`, `System.Reflection`, `dynamic`, `System.Random`, `DateTime`, `Stopwatch`, and `Environment.TickCount`. Inside `DetMath.cs` the tool permits only `MathF.Sqrt`, `MathF.Abs`, and `MathF.Floor`, because each one is an exact IEEE operation. A `MathF` transcendental is a finding in every file.
- `WhatYouCarry.Tools/BitIdentity/`: the `bit-identity` command (D-201). It runs a fixed RNG stream and a DetMath sweep over a fixed input grid, then prints the state hash. The Phase 1 roadmap first named `Tests/BitIdentity/`, and `WhatYouCarry.Tests` is a test library and not a program.
- `.github/workflows/bit-identity.yml`: one job per platform that runs the command, and a fourth job that compares the three hashes and fails on a difference (D-69, D-71).
- `.github/workflows/det-lint.yml`: one job that runs `det-lint` on the checkout. One platform is enough, because the tool reads source text.

Out of scope: any simulation type, any vector or matrix type beyond what DetMath needs.

Exit tests:

1. `RngKnownAnswer` asserts the first sixteen outputs for seed 1 against recorded values.
2. `RngStreamsDiffer` asserts that two subsystem streams from one seed do not overlap in the first ten thousand outputs.
3. `DetMathAccuracy` compares each function to a double reference over [-4 pi, 4 pi] and asserts the D-161 tolerance.
4. `DetMathRangeReduction` asserts that `Sin` and `Cos` at an angle plus many full turns equal the base angle within tolerance.
5. `StateHashOrderIsPartOfTheContract` asserts that two states with equal fields in another order hash differently (D-160, F-61).
6. `LintFailsSystemMath` runs the lint tool on a fixture that calls `System.Math.Sin` and asserts one finding.
7. `LintPassesCore` runs the lint tool on the Core project and asserts zero findings.
8. The `bit-identity` CI job reports one equal hash on three platforms.

Review focus: determinism, errors, dependencies, test quality.

Check clause: this PR creates the lint tool and the bit-identity job. It passes both (G-19).

Gate: exit tests 1 to 8 pass.

> *In plain English:* different computers give slightly different answers for functions like sine. This adds our own math that gives the same answer everywhere, and a random-number source that any seed replays. A check on every change proves the three kinds of computer agree to the last bit.

### PR-4: Logger, error context, and assertions

Status: merged 2026-09-08 as PR #15, commit `1749463`.

Scope:

- `Core/Logging/ILogSink.cs`: where a finished line goes. Core opens no file (D-211).
- `Core/Logging/JsonlLogger.cs`: one JSON object per line, with a required field set per context (D-68, D-113). Every line also carries a level and a message (D-212), and `StringBuilder` builds it (D-213). A field name is an identifier, and invalid text in one is an error. A value and a message take the replacement character in place of invalid text (D-218, F-73, F-77). The run context requires seed, floor, tick, subsystem, and entity ids. The hub context requires save versions, screen, action, and file paths. The logger throws on an absent required field.
- `Core/Logging/Invariant.cs`: `Assert` that writes a full report and continues where the caller marks the call safe, and throws elsewhere (D-112). The report holds the context, the message, and the call site that the compiler names (D-215). It walks no stack, because a stack trace changes with the build and the platform.
- `Core/Logging/ContextException.cs`: an exception type that carries the context and adds to it on each rethrow.

Out of scope: a report viewer, file rotation, the crash report UI (PR-55).

Exit tests:

1. `LogLineWithoutContextThrows` asserts that a run-context line without a seed throws.
2. `LogLineIsValidJson` parses an emitted line and asserts every required key.
3. `AssertionReportHasSeed` triggers an assertion in a run context and asserts the report contains the seed.
4. `SafeAssertionContinues` asserts that a call marked safe returns after the report.
5. `UnsafeAssertionThrows` asserts that an unmarked call throws a `ContextException`.
6. `RethrowAddsContext` catches, adds a field, rethrows, and asserts both fields on the outer catch.

Review focus: errors, input boundaries, test quality.

Check clause: none. All checks exist.

Gate: exit tests 1 to 6 pass.

> *In plain English:* every message the game writes about itself carries enough facts to replay the moment. A message without those facts is itself an error.

### PR-5: Content loader, schemas, and the string table

Status: merged 2026-09-08 as PR #17, commit `e0deb94`.

Scope:

- `Core/Content/IContentSource.cs`: where the bytes come from. Core opens no file (D-219).
- `Core/Content/JsonObjectReader.cs`: one JSON object into named text values, with `Utf8JsonReader` (D-220).
- `Core/Content/ContentLoader.cs`: takes the bytes from the source, validates each file against the validator for its type, and returns typed records (D-91, D-92, D-219). A failure names the file, the field, and the reason. An unknown field is a failure.
- One hand-written C# validator per content type, with a required-field list and an unknown-field check (D-168). Phase 1 types: `floor-template` (PR-9), `projectile` (PR-10), and `strings`.
- `Core/Content/ContentHash.cs`: SHA-256 over each content file's relative path and bytes in sorted path order, for the run record header (D-151, D-163, D-221).
- `Core/Content/Strings.cs`: the ID-keyed string table from `content/strings/en.json` (D-98). An unknown id throws.
- A lint rule in DetLint (D-98, G-8, D-222): a string literal in the Game project outside a `Strings.Get` call is a finding. An allow list covers node names and paths. The command reads Core with the determinism rules and Game with the string rule.

Out of scope: any content beyond the three Phase 1 types, localization.

Exit tests:

1. `AbsentFieldNamesField` loads a fixture with one absent field and asserts the file, field, and reason in the error.
2. `UnknownFieldFails` loads a fixture with one extra field and asserts a failure.
3. `EveryContentFileLoads` loads every file under `content/` and asserts success.
4. `UnknownStringIdThrows` asserts that an unknown id throws.
5. `ContentHashIsStable` asserts that two loads of the same files give one hash, and a one-byte change gives another.
6. `LintFlagsInlineString` runs the lint tool on a fixture Game file with an inline literal and asserts one finding.

Review focus: content, errors, Core boundary, test quality.

Check clause: none.

Gate: exit tests 1 to 6 pass.

> *In plain English:* every floor shape, projectile, and screen text lives in a data file with a strict shape. A file with a gap or an extra field fails loudly instead of a silent zero.

### PR-6: Simulation loop, intent record, recorder, and replay

Status: merged 2026-09-09 as PR #19, commit `3f7e3c2`.

Scope:

- `Core/Determinism/Crc32.cs`: the table-driven CRC-32 of the frame checksum (D-162, D-224).
- `Core/Simulation/SimulationLoop.cs`: a fixed step at 60 Hz that advances one tick per intent (D-73). The loop reads no clock. The Phase 1 state is the seed, the tick, the look sums, and the buttons (D-227).
- `Core/Simulation/Intent.cs`: the fixed 16-byte frame of D-162 (D-74, D-77).
- `Core/Simulation/SimulationVersion.cs`: one constant, initial value 1 (D-151, G-20).
- `Core/Replay/RunRecord.cs`: one JSON header line, then the fixed frames (D-163). The header holds the format version, the simulation version, the content hash, the seed, and the immutable initial state (D-151). Phase 1 writes an empty loadout, an empty tree, and no amulet assignment in the initial state, because those types do not exist yet. The schema is complete.
- `Core/Replay/IRunRecordSink.cs`: where the bytes go. Core opens no file (D-225).
- `Core/Replay/RunRecorder.cs`: writes the header, then appends one checksummed frame per tick from the first tick (D-97, G-5).
- `Core/Replay/RunReplayer.cs`: reads a record, checks the versions and the content hash, and drives the loop from the frames. It ignores the live bank and tree. A torn tail truncates to the last complete frame, with a log line that names floor 1 (D-152, D-228). A mismatch produces a report that names both versions.

Out of scope: the resume UI, the profile file (PR-31), the five-second rewind (PR-31).

Exit tests:

1. `ReplayReproducesHash` over one thousand seeds with random intents: the live run and the replay end with one state hash.
2. `TornTailTruncates` writes a record, cuts the last frame in half, and asserts the replay stops at the last complete frame with a log line.
3. `FrameChecksumDetectsFlip` flips one bit in a frame and asserts a report that names the frame.
4. `VersionMismatchReports` changes the simulation version in a header and asserts a report that names both versions.
5. `ContentMismatchReports` changes the content hash in a header and asserts a report.
6. `HeaderRoundTrip` writes and reads a header and asserts every field equal.
7. The `bit-identity` job replays one fixed record on three platforms and asserts one hash.

Review focus: determinism, replay, errors, test quality.

Check clause: none.

Gate: exit tests 1 to 7 pass.

> *In plain English:* the game runs in fixed steps and writes down its start state and every input. That record then plays any run again, so every bug becomes repeatable. A record from an older version says so instead of a silent failure.

### PR-7: Voxel world and Core collision

Status: merged 2026-09-09 as PR #21, commit `d5f20ce`.

Scope:

- `Core/World/VoxelGrid.cs`: a flat array of one-byte block ids, at most 128 by 32 by 128 (D-78, D-164). Solid or air per id, with air and raw stone as the two ids of PR-7 (D-239). The frame is the frame of Godot (D-234). A cell outside the grid is solid (D-237).
- `Core/Physics/SweptAabb.cs`: swept movement of an axis-aligned box against the grid, one axis at a time. The box is 0.6 by 1.8 by 0.6 meters, and a jump clears one block (D-27, D-80, D-165). A contact stops one skin before a block face (D-235).
- `Core/Entities/PlayerBody.cs`: a box that reads the intent's movement vector, the jump bit, and the sprint bit, applies gravity, and moves through `SweptAabb` (D-149, D-232, D-233). No health, no weapon. The intent sets the horizontal velocity on every tick (D-238), and a fall has no terminal velocity (D-240).
- Constants for gravity, walk speed, sprint speed, and jump velocity in Core (D-231).
- The loop and the replayer take the grid and the spawn point from the caller until PR-9 (D-236).
- The simulation version rises to 2, because the state gains a position (G-20).

Out of scope: enemies, the camera, procgen, any render.

Exit tests:

1. `NoTunnelAtMaxSpeed` over ten thousand random directions: a box at the maximum speed never ends inside a solid block, and never crosses a one-block wall.
2. `NoFallThroughFloor` over one thousand seeds: a box that rests on a floor block stays above it after one thousand ticks.
3. `JumpClearsOneBlock` asserts that a jump from flat ground lands on a one-block step, and fails on a two-block step.
4. `NoOverlapAfterAnyTick` over one thousand seeds with random intents: the box never overlaps a solid block.
5. `PlayerBodyIsDeterministic` replays a record and asserts one state hash.

Review focus: determinism, Core boundary, test quality.

Check clause: none.

Gate: exit tests 1 to 5 pass.

Result, 2026-09-09: every exit test passes. The sweep test drives the box at up to 20 meters per tick, far above any real speed. The jump test asserts the one-block clear and the two-block fail, and never the apex (F-83). The sweep runs Y first, then X, then Z, and a test pins the order.

> *In plain English:* the dungeon is a grid of blocks. The game itself decides how a body bumps into them, so the result is identical on every machine. A first body can already walk and jump through a test grid.

### PR-8: Camera as a Core system

Status: merged 2026-09-09 as PR #23, commit `a3bdf20`.

Scope:

- `Core/Camera/OrbitCamera.cs`: an over-the-shoulder camera with a boom (D-13, D-75). It reads the yaw and pitch sums of the loop, sweeps the boom against the grid, and derives the aim ray (D-77, D-88). Core derives it on each tick, and it holds no state (D-245). The boom sweep is a ray march along the line, with a camera radius (D-246). The aim ray starts at the camera (D-247). A positive pitch looks up (D-248), and a first march pulls the shoulder point in at a wall (D-249).
- `Core/Camera/AimAssist.cs`: a function from the aim ray, a target list, and the controller aim flag to an assisted aim ray (D-14, D-243, D-244). Phase 1 tests it with a synthetic target list, because enemies arrive in PR-16.
- The constants of D-241 and D-242: the pitch limit, the pivot, the shoulder offset, the boom, and the camera radius. The cone and the strength of D-244.
- The loop stops the pitch sum at plus or minus 8000 (D-241), and bit 8 of the buttons marks a controller aim (D-243).
- The simulation version rises to 3, because the pitch clamp changes (G-20).

Out of scope: wall fade (Game layer, PR-13), sensitivity curves (Game layer, PR-12).

Exit tests:

1. `CameraNeverInsideSolid` over one thousand seeds with random look deltas in a random grid: the camera position is never inside a solid block.
2. `AimRayIsDeterministic` replays a record with look deltas and asserts one aim-ray hash.
3. `PitchClamps` asserts that a large pitch delta stops at the limit.
4. `AssistPullsTowardTarget` asserts that the assisted ray is closer to a synthetic target than the raw ray, and equal to the raw ray with no target.
5. The `bit-identity` job replays a record with camera motion on three platforms and asserts one hash.

Review focus: determinism, replay, test quality.

Check clause: none.

Gate: exit tests 1 to 5 pass.

Result, 2026-09-09: every exit test passes. The march test walks each segment in one-millimeter steps over one thousand random grids, and it agrees with the march. The bit-identity sweep folds in the camera pose and the aim ray of every replayed tick, through an observer of the replay (F-85).

> *In plain English:* the camera is part of the simulation, not decoration, so where you look and where you aim replay exactly on every machine.

### PR-9: Procgen v1 and property tests

Status: merged 2026-09-09 as PR #27, commit `336fe4e`.

Scope:

- `content/floors/*.json`: each floor template gains `sizeX`, `sizeY`, and `sizeZ`, one size per band (D-252). The working mine of floors 1 to 5 is 48 by 12 by 48. The older workings of floors 6 to 10 are 72 by 16 by 72. The deep of floors 11 to 15 is 96 by 20 by 96. The validator rejects a size past D-164. The room count range and the difficulty budget stand (D-167).
- `content/chambers/*.json`: a new content type, one file per chamber kind, with an id, a weight, a box count range, and a box size range (D-255). One validator, with an explicit field list (D-168).
- `Core/Procgen/FloorGenerator.cs`: the mine dig plan (D-253). A seeded random walk lays a main gallery and side drifts that jitter, slope by one-block steps, and branch. A chamber is a union of boxes that overlap, smoothed by a cellular pass, with pillars left standing. Shafts and ramps join levels. The generator carves every tunnel with a brush of at least three by three, so D-166 holds by construction. The generator draws chamber kinds until the sum of weights lands inside the budget window (D-167). It reads the Procgen stream of D-159 alone, and it steps in integers, so it is deterministic by construction.
- The spawn is a standing cell at the center of the first chamber. The stairwell is one cell in the chamber with the longest walkable path from the spawn (D-256).
- `Core/Procgen/Reachability.cs`: a search over walkable cells. A cell is walkable with two air blocks above it. A move is a step of at most one block up, or any drop (D-165). A step up also needs a third air block over the start, for the jump. It gives the path lengths that place the stairwell.
- `Core/Procgen/DigCanvas.cs`: the two carve rules that keep the network walkable. The floor row of a unit is rock. No unit carves a rock cell with air above it, because that cell is the floor of an earlier unit. Air only grows and no floor goes, so every dug cell stays reachable by construction. A shaft is the one carve that removes a floor: a three by three hole in a chamber floor, with a ring of chamber floor around it.
- `Core/Simulation/StairwellTransition.cs`: at the stairwell, the interact bit descends and bit 9 ascends (D-257). Descend generates floor n+1 from the run seed and the floor number. Ascend ends the run (D-50, D-149). The loop state gains the floor number and the run end, and the hash reads both after the fields of PR-7 (D-160, G-20). An ended loop takes no intent (T-2). An intent that sets both bits ascends.
- The loop and the replayer derive the grid and the spawn from the seed, the floor number, and the content set. That closes the gap of D-236. The replay takes the content set in place of a grid and a spawn.
- The floor stream: `Rng.ForStream` takes the floor number as a third argument, and floor zero is the run stream of D-159. Floor n reads its own stream, so it never depends on the draws of the floors before it.
- The night count of exit test 1 runs when the `WYC_NIGHT_SWEEP` variable is `1`, until the night job of PR-11 owns the switch.
- PR-9 carves raw stone and air alone. The detail pass and the other block ids of D-210 are PR-59 (D-254).

Out of scope: the detail pass (PR-59), enemies, loot, the timer, the second biome.

Exit tests:

1. `EveryChamberReachable` over five thousand seeds per PR and one hundred thousand each night: every chamber is reachable from the spawn (D-116).
2. `NoChamberOverlap` asserts no two chambers share a block.
3. `StairwellReachable` asserts a path from the spawn to the stairwell, and that no chamber lies farther (D-256).
4. `BudgetWithinTolerance` asserts the sum of chamber weights within 10 percent of the floor budget (D-167).
5. `TunnelCrossSection` asserts that every tunnel cell sits inside an air cross-section three blocks wide and three blocks high (D-166).
6. `FloorSizeGrowsWithDepth` asserts floor 15 is larger than floor 1 for the same seed (D-252).
7. `GenerationIsDeterministic` asserts one grid hash for one seed, and the `bit-identity` job asserts it on three platforms.
8. `DescendAdvancesFloor` asserts that the interact bit at the stairwell generates floor n+1 from the same run seed. It asserts that bit 9 ends the run, and that a replay reproduces both (D-257).

Review focus: determinism, content, replay, test quality.

Check clause: none.

Gate: exit tests 1 to 8 pass, and the night sweep passes on one hundred thousand seeds.

> *In plain English:* this digs the mine floors from a random seed: galleries, side tunnels, chambers, and shafts, and no two floors look alike. Tests over huge numbers of seeds prove that a player can reach every chamber and the stairs down.

### PR-59: Mine detail pass

Scope:

- Collapses that fill dead ends with rubble, and pillars inside chambers (D-253, D-254).
- The block ids of D-210 beyond air and raw stone, in the order of D-259: 2 hewn stone, 3 timber beam, 4 ore vein, 5 still water, 6 rubble, and 7 plank.
- Still water is not solid (D-258). A pool is a one-block depression in a floor, and the reachability search reads water as air. While the feet stand in a water cell, the body walks and sprints at one half of the speeds of D-231 (D-261). The jump velocity takes the factor one half there too, and gravity the factor one quarter. The apex stays at one block, and the rise takes twice as long (D-262). The three factors are Core constants in `PlayerBody` (D-263).
- The blocks by band. Floors 1 to 5 take timber beams and planks on the walls of the working mine. Floors 6 to 10 take hewn stone and still water in the older workings. Floors 11 to 15 take ore veins in the deep. A wall block replaces rock that borders air and removes no air, so D-166 holds as PR-9 left it.
- `Core/Procgen/DetailPass.cs` runs after the shafts and before the stairwell: the collapses, the pools, the walls, and then the pillars. A pillar is one column of a chamber whose eight neighbors are dry chamber floor. A pool is a rectangle of two or three cells a side, and every pool cell keeps a dry chamber floor cell beside it. A collapse fills the last stamp of a walker with no dependent, in cells that walker alone dug. No path to a chamber goes with it.
- The water probe of the body reads the column under the feet: the feet cell holds water, or the body is in the air over water with air alone between (D-264). The probe holds through the jump, because a jump that loses the quarter gravity when the feet leave the cell ends below one block.
- The reachability sweep of PR-9 runs again with the detail on, so no detail closes a chamber or a tunnel. The two five-thousand-seed sweeps of exit test 1 of PR-9 and of this entry read one dig per seed.
- The simulation version rises to 5, because a floor with other blocks is another simulation (D-260, G-20). The bit-identity known answer moves with it.

Out of scope: props (PR-48), the mesher (PR-13), enemies.

Exit tests:

1. `DetailKeepsEveryChamberReachable` over five thousand seeds: the PR-9 reachability holds with the detail on.
2. `EveryBandUsesItsBlocks` asserts that a floor of each band holds the blocks of its band and none of another band.
3. `EveryBlockIdIsDeclared` asserts that the grid rejects an id outside the declared set, and that every declared id has a solid rule (D-239, D-259).
4. `DetailIsDeterministic` asserts one grid hash for one seed with the detail on, and the `bit-identity` job asserts it on three platforms.
5. `WaterSlowsTheWalkAndTheJump` asserts two facts (D-261, D-262). A body with its feet in water walks at half speed. A jump from water clears one block, and its apex comes at twice the ticks of a jump on land.
6. `EveryPoolHasAWayOut` over one thousand seeds: every water cell has a reachable floor cell beside it, one block up. The search that reads water as air then holds for a body (D-258).

Review focus: determinism, content, test quality, replay.

Check clause: none.

Gate: exit tests 1 to 6 pass.

> *In plain English:* the bare tunnels gain the look of a mine: fallen rock, timber, cut stone, ore, and water. The tests from before run again, so nothing blocks the way.

### PR-10: Projectile simulation

Scope:

- `content/projectiles/test-extremes.json`: a test-only set of projectile definitions (F-38, D-149): the slowest arc, the fastest flat shot, the longest lifetime, and the widest spread. Real weapons arrive in PR-24 and PR-43 to PR-46, and rerun these tests.
- `Core/Projectiles/ProjectileSimulation.cs`: a flat array of projectiles, fixed-step Euler integration, gravity scale, lifetime, spread, and swept collision against the grid and entity boxes (G-6). No spatial partition (D-109).
- `Core/Projectiles/ArcSolver.cs`: the launch angle for a target under gravity, with DetMath only. It reports an unreachable target.

Out of scope: damage, hit effects, enemy use, render trails.

Exit tests:

1. `NoTunnelThroughMinimumWall` fires the fastest definition at one-block walls from ten thousand random positions. It asserts a hit on the wall face and never a position beyond it.
2. `EveryProjectileTerminates` asserts every projectile ends by hit or by lifetime within its lifetime budget.
3. `ArcSolverReachesTarget` over one thousand reachable targets: the solved launch lands within one block of the target.
4. `ArcSolverReportsUnreachable` asserts a clear failure result for a target beyond range.
5. `ProjectilesAreDeterministic` replays a record with shots and asserts one state hash, and the `bit-identity` job asserts it on three platforms.
6. `EntityBoxHit` asserts a shot at a player box registers a hit on the box.

Review focus: determinism, errors, test quality.

Check clause: none.

Gate: exit tests 1 to 6 pass over the test-only definitions.

> *In plain English:* bullets and arrows are real objects that fly, drop, and can miss. Tests prove a fast bullet never passes through a wall, even before any real weapon exists.

### PR-11: Bot harness (Tier 2)

Scope:

- `WhatYouCarry.Tools/BotRunner/`: a command that runs N runs for a policy over a seed range, headless, with no sleep between ticks (D-115, D-127). It writes one JSONL run log per run through the PR-4 logger.
- Policies in Core, each a few dozen lines (D-149): `RandomWalker` holds a random movement and jump for a random number of ticks, then picks again. `GreedyDescender` walks the reachability path to the stairwell and always descends.
- Run end states: `bottom` at floor 15, `softlock` after a tick budget with no floor progress, and `crash` on any exception. The log holds the exception.
- CI: the PR job runs one hundred seeds per policy. A scheduled night job on the self-hosted macOS runner (D-157) runs five thousand seeds per policy (D-115, D-117). The night job publishes a result record with the commit, the end time, and the status. PR-58 adds the gate that reads it (D-177).

Out of scope: the `night-gate` job (PR-58, D-177), the coward, full-clearer, and timer-tester policies (PR-16 to PR-18), Tier 3.

Exit tests:

1. `RunLogHasRequiredFields` asserts policy, seed, end state, floors reached, and ticks in every run log.
2. `SoftlockIsDetected` runs a fixture policy that stands still and asserts the `softlock` end state.
3. `CrashIsLogged` runs a fixture policy that throws and asserts the `crash` end state with the exception.
4. `GreedyDescenderReachesBottom` asserts the `bottom` end state on one hundred seeds.
5. The PR job completes two hundred runs with zero crashes.
6. The night job completes ten thousand runs with zero crashes and zero softlocks.
7. `NightResultIsPublished` asserts that the night job writes a record with the commit, the end time, and the status.

Review focus: determinism, errors, input and CI boundaries, test quality.

Check clause: the `night-gate` job does not exist. PR-58 creates it (D-148, D-177, G-19).

Gate: exit tests 1 to 7 pass.

> *In plain English:* simple robots play thousands of runs every night without graphics. They find crashes and dead ends before a person ever sees them, and a bad night stops the next merge.

### PR-58: Night gate

Scope:

- A `night-gate` job in the PR workflow. It reads the result record that the PR-11 night job publishes (D-177).
- The gate passes only on a success record from a scheduled night that ran in the last 48 hours.
- The gate fails on an absent record, a stale record, a cancelled record, and a failed record (D-115, D-177).
- Each failure message names the case, the record commit, and the record time (T-2, D-113).
- This PR opens only after one scheduled night runs, so a real record exists for the gate to read (G-19).

Out of scope: the night job itself (PR-11), the Tier 3 policies (PR-16 to PR-18).

Exit tests:

1. `NightGateFailsOnRedNight` asserts a failure on a fixture record of failure.
2. `NightGateFailsOnMissingResult` asserts a failure on an empty result store.
3. `NightGateFailsOnStaleResult` asserts a failure on a fixture record older than 48 hours.
4. `NightGateFailsOnCancelledResult` asserts a failure on a fixture record of cancellation.
5. `NightGatePassesOnGreenNight` asserts a pass on a fixture success record inside the window.
6. Each failure message names the case and the record time (T-2).
7. The `night-gate` job runs on this PR and passes against the real night record (G-19).

Review focus: errors, CI boundaries, test quality.

Check clause: none.

Gate: exit tests 1 to 7 pass, and the job passes on this PR.

> *In plain English:* every merge now needs a green night from the robots. A missing or old night result stops the merge, so nobody can merge on silence.

### M-1: CI wall time per PR

Procedure: after PR-3 and each later Phase 1 PR, read the duration of each CI job per platform from the workflow run. Record ten PRs in a table in this file. If any PR job exceeds ten minutes, file a question on the seed counts in D-116.

### M-2: Night sweep wall time

Procedure: after PR-11, read the night job duration for seven nights. Record them in a table in this file. If a night exceeds six hours, file a question on the run counts in D-115 and D-116.

## 5. Sequence

One person owns the program. Items run one at a time in this order. Each PR opens only after the one before it merges.

1. Owner: receive the SSD and move the checkout to it (D-145).
2. Owner: register the runner on 2026-09-08 per `docs/runbooks/macos-runner.md` (D-157, D-171). ✅ OQ-2: D-173. ✅ OQ-16: D-175. Protection deferred: D-170.
3. ✅ PR-1 merged 2026-09-08 as PR #6.
4. ✅ PR-2 is PR #10.
5. ✅ OQ-33 to OQ-35 answered 2026-09-07: D-159 to D-161.
6. ✅ PR-3 merged 2026-09-08 as PR #12.
7. ✅ PR-4 merged 2026-09-08 as PR #15.
8. ✅ OQ-42 answered 2026-09-07: D-168.
9. ✅ PR-5 merged 2026-09-08 as PR #17.
10. ✅ OQ-36 and OQ-37 answered 2026-09-07: D-162 and D-163.
11. ✅ PR-6 merged 2026-09-09 as PR #19.
12. ✅ OQ-38 and OQ-39 answered 2026-09-07: D-164 and D-165.
13. ✅ PR-7 merged 2026-09-09 as PR #21.
14. ✅ PR-8 merged 2026-09-09 as PR #23.
15. ✅ OQ-12 answered 2026-09-08: D-210. ✅ OQ-40 and OQ-41 answered 2026-09-07: D-166 and D-167. ✅ OQ-120 to OQ-125 answered 2026-09-09: D-252 to D-257.
16. ✅ PR-9 merged 2026-09-09 as PR #27.
17. PR-59.
18. PR-10.
19. PR-11.
20. One scheduled night runs on the runner (D-177).
21. PR-58.
22. M-1 table complete. M-2 table complete.
23. **← GATE 1 (foundation).** Every exit test in this file passes. The bit-identity job, `dotnet test`, and the night sweep are green. The owner signs the gate in `docs/decisions.md`.

## 6. Open questions

The register is `docs/questions.md` (D-144). These questions bind Phase 1. Each names the PR it blocks.

Open:

- None. OQ-99 is open, and it blocks nothing.
- OQ-2 resolved 2026-09-07 by D-173: .NET 10 LTS. D-173 revises D-169.
- OQ-16 resolved 2026-09-07 by D-175: the attribution option is in the repository. D-175 revises D-172.

Resolved 2026-09-07:

- OQ-31 (D-157, D-171): the self-hosted macOS runner. Registration on 2026-09-08 per the runbook.
- OQ-32: branch protection on `main`. D-170 revises D-158. Protection waits until launch, and it is a convention until then.
- OQ-33 to OQ-35 (D-159 to D-161): the RNG, the state hash, and the DetMath target. PR-3.
- OQ-36 and OQ-37 (D-162 and D-163): the intent and run record layouts. PR-5 and PR-6.
- OQ-38 and OQ-39 (D-164 and D-165): the grid limits, the player box, and the jump. PR-7 and PR-9.
- OQ-40 and OQ-41 (D-166 and D-167): the corridor and the budget. PR-8 and PR-9.
- OQ-42 (D-168): the validator form. PR-5.

Resolved 2026-09-09:

- OQ-92 to OQ-98 (D-224 to D-230): the CRC-32, the record sink, the frame wording, the loop state, the torn-tail floor, the empty initial state, and the allowlist. PR-6.
- OQ-99 is open, and it blocks nothing. It asks for a lint rule on an enum inside an interpolation.
- OQ-100 to OQ-103 (D-231 to D-234): the motion constants, the button bits, the movement mapping, and the coordinate frame. PR-7.
- OQ-104 to OQ-109 (D-235 to D-240): the body position and the contact skin, the world input, the grid edge, air control, the block ids, and the terminal velocity. PR-7.
- OQ-110 to OQ-116 (D-241 to D-247): the pitch limit, the camera geometry, the controller aim flag, the assist shape, the camera state, the boom sweep, and the aim ray origin. PR-8.
- OQ-117 and OQ-118 (D-248 and D-249): the pitch sign and the shoulder march. PR-8.
- OQ-119 (D-251): the visible review gate line. The fix PR #25.
- OQ-120 to OQ-125 (D-252 to D-257): the floor size, the room shape, the PR-9 split, the chamber templates, the spawn and the stairwell, and the stairwell choice. PR-9 and PR-59.

Resolved 2026-09-08:

- OQ-73 (D-200): the `Pow` contract. An integer exponent. PR-3.
- OQ-74 (D-201): the location of the bit-identity program. `WhatYouCarry.Tools`. PR-3.
- OQ-75 (D-202): the parser of the lint tool. The C# compiler API. PR-3.
- OQ-76 (D-203): the conflict inside D-161. A fold to [-pi/4, pi/4] and a quadrant. PR-3.
- OQ-12 (D-210): the biome is a collapsed deep mine. PR-9 and PR-59.
- OQ-126 to OQ-131 (D-258 to D-263): still water, the block ids, the version for a layout change, and the water speed, jump, and constants. PR-59.
- OQ-132 (D-264): the water probe reads the column under the feet. PR-59.
