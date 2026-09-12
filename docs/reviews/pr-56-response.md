# PR-56 review response

Date: 2026-09-12

## Identity

- PR: 56
- Review: `docs/reviews/pr-56.md`, verdict `Changes required` for head `9236744`.
- Response head: the commit that holds this file, on `feat/pr-14-texture-generator`.

## P2-1: Unreadable input files crash the texture command

Disposition: full merit.

Evidence: the trigger reproduces at `9236744`. A copy of `content/textures` with one input at mode 000 gave exit code 134 and an unhandled `System.UnauthorizedAccessException` for each of three inputs: `rules/raw-stone.json`, `palette.json`, and the `rules/` directory. The directory is a third trigger of the same cause. The contract is the exit code summary of `TextureGenCommand.Run`, which gives exit code 1 for a bad palette or rule, and T-2.

Correction: `WhatYouCarry.Tools/TextureGen/TextureGenCommand.cs`.

- `ReadFile` catches `IOException` and `UnauthorizedAccessException` around the palette read, and it throws a `ContextException` that names the file with the platform message.
- `ReadRules` holds the directory listing and every rule read in one boundary. It catches the same two kinds and throws a `ContextException` that names the rule directory. The platform message names the exact path, so the listing and a rule read share one tested catch.
- The summaries of `Run` and `AtlasBytes` name the read failure. `Run` needs no other change, because it already turns a `ContextException` into exit code 1.

Regression check: `TextureGenTests.CommandReportsAnUnreadableRule` and `TextureGenTests.CommandReportsAnUnreadablePalette`. Each test makes one input unreadable, proves that the user cannot read it, runs the command with the error output captured, and asserts exit code 1, the file name in the output, and no atlas. Linux and macOS remove every permission from the file. Windows holds the file open with no share.

- At `9236744` with the two tests added and the command unchanged: 2 failed, each with the unhandled `UnauthorizedAccessException` from `Run`.
- With the correction: 2 passed. The focused run of `TextureGenTests`, `ContactSheetTests`, and `ConsoleCollectionTests` passed 57 tests.
- The three manual triggers now end with exit code 1, a message that names the path, and no atlas.
- The rule directory has no automated test of its own, because Windows gives no plain way to make a directory unreadable. Its listing shares the catch of the rule test, and its manual trigger on macOS ended with exit code 1.

## Verification

- `dotnet build WhatYouCarry.slnx`: 0 warnings, 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build`: 761 tests, 0 failures, with the two Smoke tests on the local Godot build.
- `det-lint`: 0 findings, Core 0 in 61 files, Game 0 in 26 files.
- `ste-check`: 0 findings in 15 files.
- Core, Game, and content did not change, so `bit-identity`, `asset-qa`, and the committed atlas stay as the review recorded them.

## New ids

None. The correction adds no decision, no finding, and no question.
