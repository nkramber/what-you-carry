# PR-109 review response

Date: 2026-09-26

Author: Claude Code. Review: `docs/reviews/pr-109.md`, round 1 at `6b327c9`, verdict `Changes required`.

## P2-1: The reader accepts a CRC field before later header fields

Disposition: full merit.

Evidence: at `6b327c9`, `RunRecord.CheckHeaderCrc` found the last `,"headerCrc":` marker and compared the CRC-32 of the bytes before it with the stored value. It never checked that the field ends the line, and `ContentValidator.Check` accepts any field order. A header with `headerCrc` before `seed`, and the CRC of the bytes before that field, read as a valid header with any seed after it (D-637).

Correction: `69c946a`. `WhatYouCarry.Core/Replay/RunRecord.cs` requires the bytes after the marker to be the CRC digits and the closing brace alone. A field after the CRC, or any other byte, is a content error that names the header and the field `headerCrc`.

Regression check: `ReplayTests.ACrcFieldBeforeAnotherFieldIsAnError` builds a header with the CRC field before `seed`, with the CRC of the bytes before it, and a changed seed and an unchanged seed after it. Both rows failed on the reader of `6b327c9` and pass on `69c946a`. The same test rejects a space before the closing brace. `EachFlippedBitOfTheHeaderIsAnError` stays green. `det-lint` gives 0 findings, and the bit-identity answer stays `e202e84e0f5c188a`, because the writer did not change.

## New ids

No new D-# or F-# id. The correction restores D-637 as written.

## PR head

The correction commit is `69c946a`. The handoff commit of session 278 follows it.

## Round 4 at `d32da6b`

### P2-2: The poll marks a night handled when it only dispatches a notice

Disposition: full merit.

Evidence: at `d32da6b`, `notify` in `.github/scripts/night-fixer.sh` returned 0 when GitHub accepted `gh workflow run notify.yml`. A dispatch only puts a run in the queue, and the `notify` job fails when the send fails (D-642). The failed-setup path then marked the night handled with no delivered notice (D-645).

Correction: the new script `.github/scripts/notify-owner.sh` dispatches `notify.yml` with a unique id, which the run name of `notify.yml` carries. It finds that run, and waits for it with `gh run watch --exit-status`. It exits 0 only when the run succeeds. The poll and the prompt send each notice through it, so a failed run keeps the night open for the next poll. The launchd job now copies the whole folder `.github/scripts` of `origin/main`, so the helper lies beside the poll.

Regression check: `NightFixerTests.AFailedNoticeRunKeepsTheNightOpen` gives the fake a dispatch that GitHub accepts and a run that fails. The night stays out of `handled`. The test failed on the poll of `d32da6b`.

### P2-3: A poll can remove a live lock before its PID is written

Disposition: full merit.

Evidence: at `d32da6b`, the lock was a directory, and the poll wrote `lock/pid` after it made the directory. A second poll in that gap read no process id, took the lock as stale, and removed it.

Correction: the lock is a symbolic link whose target is the process id. `ln -sn` makes the link and its target in one call, so no lock exists with no process id. A lock that is no link, or whose target is no number, stops the poll with an error and stays. A stale lock goes only when it still names the same dead process. That check and the removal were still two open steps, and the gitar pass of `50862e5` showed that a second poll could remove a live lock between them. The correction after `50862e5` puts the two steps behind the guard `lock.reap`, which one `mkdir` takes, so one poll alone removes a stale lock. A guard older than 10 minutes stops each poll with an error. `NightFixerTests.AGuardOfAnotherPollLeavesTheStaleLock` and `AnOldGuardStopsThePoll` failed on the poll of `50862e5`.

Regression check: `NightFixerTests.ALockWithNoProcessIdStaysAndStopsThePoll` puts a lock with no process id in place, the state of the gap. The poll stops with exit code 1 and keeps the lock. It failed on the poll of `d32da6b`, which removed the lock and read on. `ALockOfAnotherFormStopsThePoll`, `ALiveLockStartsNoSession`, and `AStaleLockGoes` test the other lock states.

### A defect found during the correction: F-177

The full suite of the correction failed once in `SmokeSessionTests.HudConstructsHeadless`: the engine crashed after the end line. Under the load of eight busy processes, 3 of 100 smoke sessions at `adb371a` crashed at exit, and 0 of 100 on `main`. Without the two lines of F-161, 1 of 100 crashed. Each crash was in the .NET finalizer, which freed the leaked `ArrayMesh` wrappers of the chunk and box meshes after the engine shut down. The CI abort of `smoke-macos-arm64` at `6a2d1a8` had the same cause.

The owner chose the fix of the cause in this PR. `ArrayMeshBuilder.BuildInto` gives the node the mesh and disposes the managed wrapper at once, and its three callers use it. With the fix, 0 of 200 smoke sessions crashed under the same load, and no run left a leak report. `GameShapeTests.EachMeshWrapperGoesWhenItsNodeTakesTheMesh` holds the rule.

## PR head after round 4

The corrections are in the commit after `adb371a`. The full suite and the new tests ran before the push.
