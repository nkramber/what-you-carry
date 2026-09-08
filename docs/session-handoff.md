# Session handoff

Rule (D-146): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

## Session 27: 2026-09-08, Claude Code

Author: Claude Code
Session: the session end gate for the `pr-review` skill. Branch `docs/pr-review-push-gate`, PR #11.

### What this session did, and why

- The owner asked for a fix after the reviewer left its commit unpushed on the shared checkout twice on PR #10 (F-59). The skill said "push" in three places, with no verification step and no evidence trail.
- Added a "Session end gate" section to the skill. Four commands after the commit, and the evidence comes from the remote: `git status --short --branch` shows no `[ahead N]`, and `gh pr view --json headRefOid` equals `git rev-parse HEAD`.
- Added the push line as a required part of the Verification section in the review skeleton. A record with no push line is incomplete.
- Added the failure path. A denied push does not end the session. The session asks the owner to approve it and says in the handoff that the record is unpushed. A sandbox that blocks the network denies a push in silence, so the status line is the evidence and not the push output.
- Added the start-of-session check for both roles. If the checkout is ahead with the other provider's commit, push it first and record that.
- The owner also chose the rule for every session, not only reviews. D-199 records it, and revises in part D-146: the state of the build names the remote head. `CLAUDE.md` and `AGENTS.md` carry the rule in the session handoff section.
- This is the second PR of one harness invocation, after PR #10, on the owner instruction. D-121 names one PR per session.

### State of the build

- `main` is at `d5eb298`, the squash merge of PR #10. The branch holds one commit above it, and this entry is in that commit.
- Remote head: `origin/docs/pr-review-push-gate` at the commit that holds this entry, checked with the session end gate before the session ended.
- `dotnet build` and `dotnet test` pass: 63 tests, 0 failures. The checker reports 0 findings.

### In flight

PR #11 changes only `docs/`, `CLAUDE.md`, `AGENTS.md`, and `.claude/skills/`, so the `review-override` label covers it (D-190). The owner asked for the label in the instruction, and this session added it.

### Traps and gotchas

- The evidence for a push is the remote, never the local checkout. A sandbox denial gives no git error.
- A revision of one part of a decision needs the `Revised in part by` marker on the old row, and the reference check does not flag it (D-186).

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner merges PR #11 with the `review-override` label. Then a new session starts PR-3: the seeded RNG, DetMath, the lint tool, and the bit-identity CI job.

## Session 26: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #10. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Re-reviewed PR #10 at effective head `b9bc6cd` against base and merge base `94aadc5`.
- Confirmed that P2-1 to P2-4 are resolved.
- Updated `docs/reviews/pr-10.md` to `Ready for owner merge`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 63 tests and 0 failures.
- The repository checker reports 0 findings in 15 files.
- The Godot 4.7.2 headless build check passes with the installed executable.

### In flight

PR #10 is ready for owner merge.

### Traps and gotchas

- The mask pass must protect every delimiter inside an earlier masked span.
- The review commit is metadata. The review head remains `b9bc6cd` under D-184.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner merges PR #10. Then PR-3 starts with the seeded RNG, DetMath, the lint tool, and the bit-identity job.

## Session 25: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #10 repeat review. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Read the repeat review in `docs/reviews/pr-10.md`. P2-1 to P2-3 are resolved, and P2-4 is new.
- P2-4 has full merit. The quote pass masked the text inside a quote, but it left a quoted `)` as a plain character, so the parenthesis pass paired the outer `(` with it. `MaskChar` now masks both parentheses, both quote forms, and the backtick inside a span, and `Unmask` restores them. Four new assertions in `NestedParenthesesAreOneOpaqueWord` cover it.
- Updated `docs/reviews/pr-10-response.md` with the P2-4 disposition.
- The review commit `712b2e0` was on the local checkout and not on the remote, as `29b1066` was before it. This session pushes it with the response commit.

### State of the build

- `dotnet build` and `dotnet test` pass on the Mac Mini: 63 tests, 0 failures. The checker reports 0 findings in 15 files.
- PR #10 is open. The effective head is the commit that holds this entry, because it holds the code correction too.

### In flight

PR #10 needs a repeat review at the new effective head.

### Traps and gotchas

- A mask pass must mask every delimiter inside its span, not only the sentence punctuation. A later pass reads any character that the earlier pass left plain.
- An unbalanced quote or an unclosed parenthesis stays plain text. Each token then counts on its own.
- The reviewer's commit was unpushed for the second time. Check `git status` before you start, and push it.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #10 at the new effective head and updates `docs/reviews/pr-10.md`.

## Session 24: 2026-09-07, Codex

Author: Codex
Session: repeat review of PR #10. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Re-reviewed PR #10 at effective head `d55666d` against base and merge base `94aadc5`.
- Confirmed that the three prior P2 findings are resolved.
- Found P2-4. A quoted close parenthesis can end an outer parenthesized span.
- Updated `docs/reviews/pr-10.md` with the new finding and the verdict `Changes required`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 63 tests and 0 failures.
- The repository checker reports 0 findings in 15 files.
- The Godot check did not complete because Godot could not write its macOS support file.

### In flight

PR #10 needs P2-4 corrected and a repeat review at the new effective head.

### Traps and gotchas

- The masking passes run in sequence. A later pass can read delimiters that an earlier pass already made opaque.
- `FindSpanEnd` fixes nested parentheses of one type. It does not protect against delimiters from another span type.
- The review commit is metadata. The review head remains `d55666d` under D-184.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-4, add the regression tests, and request another repeat Codex review.

## Session 23: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #10 review. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Read the three P2 findings in `docs/reviews/pr-10.md` and assessed each against the evidence.
- P2-1 has partial merit. The splitter requires a space after a colon on purpose: the `ste-writing` skill holds a bare URL in prose, and a split at every colon cuts it, and cuts every time and ratio. The roadmap and the skill over-claimed with "everywhere". Both now state the space condition, and `ColonInsideAWordDoesNotEndASentence` covers it.
- P2-2 has full merit. Nested parentheses ended the span early. `FindSpanEnd` now counts depth, and `NestedParenthesesAreOneOpaqueWord` covers it.
- P2-3 has full merit. A trailing argument after `--root` was silent. The loop now rejects every argument that is not a `--root <value>` pair, with exit 2 and a message that names the argument. Three new assertions cover it.
- Wrote `docs/reviews/pr-10-response.md`.
- The review commit `29b1066` was on the local checkout and not on the remote. This session pushes it with the response commit.

### State of the build

- `dotnet build` and `dotnet test` pass on the Mac Mini: 63 tests, 0 failures. The checker reports 0 findings in 15 files.
- PR #10 is open. The effective head is the commit that holds this entry, because it holds the code corrections too.

### In flight

PR #10 needs a repeat review at the new effective head.

### Traps and gotchas

- The reviewer commits on this checkout when both providers share it. Check `git status` for an unpushed commit before you start, and push it.
- `Sentence.Words` holds the masked tokens. Unmask a word before you compare it to text.
- Count the words of a test sentence with the splitter, not in your head. Three expectations in this PR were off by one.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session re-reviews PR #10 per the repeat review procedure and updates `docs/reviews/pr-10.md` to the new effective head.

## Session 22: 2026-09-07, Codex

Author: Codex
Session: PR-10 review. Branch `feat/pr-2-ste-check`.

### What this session did, and why

- Reviewed PR #10 at effective head `3ea5f23` against base and merge base `94aadc5`.
- Confirmed the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Found three P2 defects in the sentence splitter and command argument parser.
- Wrote `docs/reviews/pr-10.md` with the verdict `Changes required`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 61 tests and 0 failures.
- The repository checker reports 0 findings in 15 files.
- The Godot 4.7.2 headless build check passes with the installed executable.

### In flight

PR #10 needs the three P2 findings corrected and a repeat review at the new effective head.

### Traps and gotchas

- The checker only treats a colon as a sentence end when whitespace or the line end follows it. The project rule says that every colon ends a sentence.
- Nested parentheses do not remain one opaque word. The mask pairs the first opening parenthesis with the first closing parenthesis.
- `ste-check` ignores a trailing argument after a valid `--root` pair and returns the repository result.
- The review commit is metadata. The review head remains `3ea5f23` under D-184.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P2-1 to P2-3, add the regression tests, and request a repeat Codex review.

## Session 21: 2026-09-08, Claude Code

Author: Claude Code
Session: PR-2, the STE checker. Branch `feat/pr-2-ste-check`, PR #10.

### What this session did, and why

- Started PR-2 after the owner merged PR #9. Session 19 named it as the next action, and nothing blocked it.
- Wrote the `ste-check` command of `WhatYouCarry.Tools` (D-130, D-139). One reader turns a Markdown file into prose lines, one splitter turns a line into sentences and words, and one rules class holds the seven STE rules. The reference check (D-178, D-186) and the session number check (D-187) are two more classes. `RepositoryCheck` runs all three over one checkout.
- The splitter masks text in backticks, double quotes, and parentheses. Each span is one opaque word (8.5, 8.6), and no grammar rule reads inside it. A colon ends a sentence everywhere (8.4).
- Wrote 25 tests. Exit tests 1 to 9 of the roadmap each have a test, and the command test runs `Program.Main` on a throwaway checkout for the exit codes 0, 1, and 2.
- Wrote `.github/workflows/ste-check.yml` with the one job `ste-check`. It runs on Linux only, because the checks read text.
- The first run found 77 findings in 15 files: 52 passive, 15 helper verbs, 5 over 25 words, and 5 -ing forms. Two were rule gaps. A hyphenated identifier in a skill front matter matched the -ing rule, and the noun "finding" matched it after "per". Both are exclusions now, with tests. The other 75 were real, and each sentence is rewritten with the same meaning.
- Rewrote the checker section of the `ste-writing` skill. It gives the command, a table of the rule ids, the exemptions, and the Markdown conventions the checker needs.
- Marked PR-1 and PR-2 done in the design doc and the roadmap. PR-1 was still marked planned after its merge.

### State of the build

- `main` is at `94aadc5`. The branch holds one code commit, `3ea5f23`, and this entry.
- `dotnet build` and `dotnet test` pass on the Mac Mini: 61 tests, 0 failures. The checker reports 0 findings in 15 files.
- PR #10 is open. CI, `ste-check`, and `review-gate` run on it from the trunk workflows.

### In flight

PR #10 waits for a Codex review (T-4). The `review-gate` check shows grey until the review record lands.

### Traps and gotchas

- The passive rule is a heuristic: an auxiliary, an optional adverb, and a word that ends in "ed" or is on the irregular list. "is required" and "is closed" are findings. Rewrite with the actor as the subject.
- "have" before a participle is a complex tense, so "the docs have dated records" is a finding. Use "hold".
- The checker reads each line alone. A sentence that wraps to a second line counts as two short ones.
- The reference check skips the exempt paths. A dated record cites old decisions as history, and a rewrite to name the reviser falsifies it.
- `perl -p` reads one line at a time. A multi-line replacement needs `-0`, and an argument that starts with a hyphen needs `--` before it. Both failed silently this session before the fix.
- A `## Procedure` or `## Sequence` heading gives every numbered item below it the 20-word limit, until the next heading at any level.
- To add a technical name that ends in -ing, add it to `NotIngForms` in `SteRules.cs` with a test.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

A Codex session reviews PR #10 per `.claude/skills/pr-review/SKILL.md` and writes `docs/reviews/pr-10.md`. After the merge, PR-3 starts: the seeded RNG, DetMath, the lint tool, and the bit-identity CI job.

## Session 20: 2026-09-08, Codex

Author: Codex
Session: repeat review of PR #6. Branch `feat/pr-1-scaffold`.

### What this session did, and why

- Re-reviewed PR #6 at effective head `03e6a29` against base and merge base `546a70a`.
- Confirmed P1-1 is fixed in `9624cfa`. The `pull_request_target` workflow runs the base-branch evaluator and fetches the PR head as data only.
- Confirmed P1-2 remains an accepted risk under D-198. The output names the commit that last changed the review file, and the repository cannot prove provider identity with its shared unsigned commits.
- Found no new finding. Updated `docs/reviews/pr-6.md` to `Ready for owner merge`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 35 tests and 0 failures.
- GitHub reports passing Linux, Windows, and macOS CI at head `03e6a29`.
- The review-gate check does not run on PR #6 before the workflow reaches `main`, as recorded in D-197 and F-58.

### In flight

The owner can merge PR #6. After the merge, run the adversarial proof against `main` that D-197 requires.

### Traps and gotchas

- The `pull_request_target` workflow is available only after its file reaches the default branch.
- The review-record identity risk remains accepted under D-198.
- The effective head is `03e6a29`, because the latest register changes are outside the metadata paths.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner merges PR #6. Then run the adversarial proof against `main` and record its result.

## Session 19: 2026-09-08, Claude Code

Author: Claude Code
Session: answer the PR #6 review. Branch `feat/pr-1-scaffold`.

### What this session did, and why

- Read the two P1 findings in `docs/reviews/pr-6.md` and assessed each against the evidence.
- P1-1 has full merit. The `pull_request` event ran the tool from the PR head with `checks: write`. The owner chose `pull_request_target` (D-197). The workflow now checks out the base, fetches the PR head as data, and runs the base-branch tool. `ReviewGateRunsOnPullRequestTarget` covers it.
- P1-2 has partial merit. The rewrite of a review file reproduces, and the requested identity check cannot be built, because every commit has one identity and no signature. The owner accepted the risk under D-190 (D-198). The output now names the commit that last changed the review file. Two tests cover it.
- Opened PR #7, a throwaway adversarial PR against the PR-1 branch, to prove the trusted evaluator live. GitHub produced no run. The events reference says `pull_request_target` triggers only when the workflow file exists on the default branch (F-58). Closed PR #7 and deleted the branch.
- Wrote `docs/reviews/pr-6-response.md`, D-197, D-198, F-56 to F-58, and the roadmap corrections.
- After the owner merged PR #6 as `a3b20e2`, opened PR #8, a throwaway adversarial PR against `main`. The trusted tool from `main` answered `neutral` on the adversarial head, and the head's approval never posted. Run 34180347093. Closed PR #8 and deleted the branch. The response file records the proof.

### State of the build

- `main` is at `a3b20e2`, the squash merge of PR #6. The CI workflow and the review-gate workflow are on the trunk.
- `dotnet build` and `dotnet test` pass on the Mac Mini: 35 tests, 0 failures. CI passed on the three platforms at `9624cfa`.
- The `review-gate` check now runs on every PR against `main` from the trunk workflow (D-197). PR #8 proved it.

### In flight

PR #6 is merged. The branch `docs/d-197-proof` records the proof in the response file and this entry. It changes documents only, so the owner adds the `review-override` label (D-190). That label run is the first live use of the override path.

### Traps and gotchas

- `pull_request_target` reads the trigger from the default branch. A workflow on a feature branch never runs for that event, and a PR against that branch gives no error, only silence (F-58).
- A `pull_request_target` run lists the base branch as its head branch in `gh run list`. Do not filter by the PR branch.
- The gate cannot see PR #6 until the merge. The owner merges PR #6 on the review record alone, as for PR #1 to PR #5.
- The identity of a commit proves nothing about the provider (D-198). Read the "Review file last changed by" line in the check output.
- The adversarial proof must never merge. Close the PR and delete the branch after the run.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

The owner adds the `review-override` label to the proof PR and merges it. Then a new session starts PR-2, the STE checker (D-121: one session, one PR).

## Session 18: 2026-09-08, Codex

Author: Codex
Session: review PR #6. Branch `feat/pr-1-scaffold`.

### What this session did, and why

- Reviewed PR #6 at head `39db1f9` against base and merge base `546a70a`.
- Confirmed the opposite-provider gate. The PR author is Claude Code, and the reviewer is Codex.
- Found two P1 defects in the review gate. The workflow runs PR-controlled evaluator code with `checks: write`. A PR author can replace an approved review record in a metadata-only commit.
- Wrote `docs/reviews/pr-6.md` with the verdict `Changes required`.

### State of the build

- `dotnet build WhatYouCarry.slnx -m:1` passes with 0 warnings and 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1` passes with 32 tests and 0 failures.
- GitHub reports passing Linux, Windows, and macOS CI for PR #6. The custom review-gate check is neutral until the review record approves the head.
- The Godot command did not run because `Godot` is not on this checkout's command path.

### In flight

PR #6 needs both P1 findings corrected and a repeat review at the new effective head.

### Traps and gotchas

- The review-gate workflow checks out the PR head and runs `WhatYouCarry.Tools` from that head.
- The effective-head rule excludes `docs/reviews/`, so it does not detect a review-file replacement.
- PR #6 has GitHub base `546a70a`, while `main` now points to `4ec9708`.

### Open questions that block progress

No new owner question. OQ-12 remains open for PR-9.

### Next concrete action

Correct P1-1 and P1-2, then request a repeat review of PR #6.
