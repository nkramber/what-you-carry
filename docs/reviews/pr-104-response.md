# PR-104 review response

Date: 2026-09-25

Author: Claude Code. Review: `docs/reviews/pr-104.md`, round 1 at `4029e36`, verdict `Changes required`.

## P1-1: A malformed closed finding status passes as known

Disposition: full merit.

Evidence: at `4029e36`, `ReviewFindings.IsKnownStatus` matched a status by its first word. `Status: fixed.` and `Status: fixed without evidence.` parsed, and `ReviewFinding.IsOpen` read each one as closed. The form of `findings.md` is `open`, `fixed in` a hash in backticks, `accepted risk, D-#`, or `withdrawn`, each with a period. Each of the 49 status lines in the review records of the repository has one of these four forms.

Correction: `WhatYouCarry.Tools/CodexReview/ReviewFindings.cs` reads each status against the complete forms with one expression. A hash has 7 to 40 hexadecimal characters, and a decision id has one or more digits. Any other status is a `FormatException` that names the finding, the status, and the four forms, and `codex-review` then gives the Fault exit (F-125, D-514).

Regression check: `CodexReviewTests.AFindingWithAnUnknownStatusIsAFault` has six new rows: `fixed`, `fixed without evidence`, `fixed in 2222222` with no backticks, a fixed form with more text, `accepted risk` with no decision, and `withdrawn for now`. On the parser of `4029e36`, the six rows failed. `EveryKnownStatusParses` has two new rows, a full hash and `D-1`, and the four earlier rows still pass. `EveryRepositoryFindingsSectionParses` still passes on each record.

## P1-2: Fence parsing can expose a fake approving section

Disposition: full merit.

Evidence: at `4029e36`, `ReviewRecord.LinesOutsideFences` toggled its state on each line that starts with three backticks or three tildes. Two tilde lines inside a backtick fence therefore closed and opened it again, and `FindHead` and `FindVerdict` read the fake Identity and the fake Verdict between them.

Correction: `WhatYouCarry.Tools/ReviewGate/ReviewRecord.cs` keeps the character and the length of the opening run. Only a line of the same character, with a run at least as long and nothing after it, closes the fence, as in Markdown (F-116, D-179, D-269).

Regression check: `ReviewGateRulesTests.ReviewGateKeepsATildeLineInsideABacktickFence` puts two tilde lines and a fake approving Identity and Verdict inside a backtick fence, above a real `Blocked` record. The parse gives `Blocked` and the real head. `ReviewGateClosesAFenceOnAMatchingRunAlone` holds the boundary: a shorter run of backticks inside a four-backtick fence stays inside, and a longer run closes a three-backtick fence. On the parser of `4029e36`, both tests failed.

## The verification note

The record says that Smoke and Bit identity passed at `756d539`. At `756d539` and at `35c389c`, both workflows skipped their jobs, because each commit after `4029e36` changes documents alone (D-474). The runs on `7207164`, the first head with the code, were cancelled by the next push (D-356). A re-run on `7207164` passed: Bit identity run 36136980426 on Windows, macOS, and Linux with the compare job, and Smoke run 36136980200 on the three platforms.

## Checks of the correction

- The review gate and review command tests passed 176 of 176.
- The full suite, Smoke included, is in the handoff entry of this round.

## New ids

None. The corrections restore the contracts of F-116 and F-125.

## Final head

The correction commit is the new effective head. The handoff entry of this round names it.

## Round 2

Review: round 2 at `d9c6413`, verdict `Changes required`. Round 2 closed P1-1 and P1-2.

### P1-3: A backtick in a fence info string can expose a fake approval

Disposition: full merit.

Evidence: at `d9c6413`, `ReviewRecord.LinesOutsideFences` opened a fence on each run of three backticks. Markdown opens no backtick fence whose info string holds a backtick, so a line such as three backticks, `c`, and a backtick opened a fence in the gate and none in the view. The next line of three backticks then closed the fence in the gate, and the gate read a fake approval that the view hid.

Correction: `WhatYouCarry.Tools/ReviewGate/ReviewRecord.cs` opens no backtick fence when a backtick follows the run on the same line (F-116).

Regression check: `ReviewGateRulesTests.ReviewGateOpensNoFenceOnABacktickInTheInfoString` gives the visible `Blocked` verdict for the trigger of the finding, and a fence with a plain info string still opens. On the parser of `d9c6413`, the test failed. The review gate and review command tests pass 180 of 180.

### P2-1: A const text in another file can bypass the string lint

Disposition: full merit.

Evidence: at `d9c6413`, `GameStringScan.ScanText` read the const string names of one file alone, so `label.Text = Texts.Died` with the const in another file gave no finding.

Correction: `WhatYouCarry.Tools/DetLint/GameStringScan.cs` joins the const string names of every Game file before the scan of each file. `ScanSources` holds the rule, and `ScanText` reads one text through it (G-8, D-98, F-132).

Regression check: `GameStringScanTests.AConstOfAnotherFileThatReachesATextIsAFinding` gives one finding for the const of another file that reaches a text member, and none for the const id that `strings.Get` reads. With the names of each file alone, the test failed. `det-lint --root .` still reports 0 findings.

### The round end

The review pushed its record at 18:10 UTC, and its process stayed open with no output for ten minutes. The session stopped the process. The record on the branch is complete.
