# PR-34 response

Date: 2026-09-10

## Identity

- PR: 34
- Reviewed head: `0536f7e`
- Response head: `f6ca5f6`
- Branch: `feat/pr-11-bots`

## Push check

`git fetch` and `git pull --ff-only` brought the review commits `00e4c62` and `e601b43` into the checkout, and `git status --short --branch` showed it level with the remote, so no push came first (F-59).

## P1-1: Bot tests race with global console capture

Disposition: full merit.

Evidence: `BotTests.RunLogHasRequiredFields` calls `Program.Main`, and `bot-run` writes its summary line to the process console. `BitIdentityTests.TheCommandPrintsTheHashAlone` redirects that console to a test-local writer and reads the hash from it. xUnit runs test classes in parallel, so the writer of one test could take the line of the other, as the review saw once in a full run. The same holds for `ReviewGateGitTests` and `SteCheckTests`, which call commands too.

Correction: the four test classes that touch the console carry `[Collection(ConsoleCollection.Name)]`, so xUnit runs them one after another and no capture reads the line of another test. `ConsoleCollectionTests.EveryConsoleTestIsInTheCollection` reads every test source and fails on a class that calls a command or redirects the console outside the collection, so a new command test cannot bring the race back. The commands and the tests are as they were otherwise.

Regression check: `dotnet test WhatYouCarry.slnx --no-build -m:1` three times in a row on the corrected head. Each run: 518 passed, 0 failed, 0 skipped. The count is one above the reviewed head, for the shape test.

## New ids

F-90 records the finding. No new decision and no new question.

## Final head

The effective head is `f6ca5f6`, which holds the collection, the shape test, and F-90. The commit after it holds this response and the Session 91 handoff entry.
