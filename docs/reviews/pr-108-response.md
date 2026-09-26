# PR-108 review response

Date: 2026-09-26

Author: Claude Code. Review: `docs/reviews/pr-108.md`, round 1 at `4edcd2c`, verdict `Changes required`.

## P2-1: Reject a short PNG header before reading its fields

Disposition: full merit.

Evidence: at `4edcd2c`, the `IHDR` branch of `ScreenshotPng.Read` read 13 bytes at fixed offsets and did not check the chunk length. A signature and a header chunk of 0 data bytes, with nothing after it, pass the chunk loop. The fixed read then passes the end of the file, and `IndexOutOfRangeException` leaves the command, which catches `ContextException` alone (T-2).

Correction: `e4d3543`. `WhatYouCarry.Tools/TextureTrace/ScreenshotPng.cs` rejects a header chunk that does not hold 13 bytes, with a `ContextException` that names the file, the byte of the chunk, and its length.

Regression check: `TextureTraceTests.ScreenshotReaderRejectsAnotherForm` has the case `header`. On the old reader, the case failed with `IndexOutOfRangeException` in place of `ContextException`. With the correction, the case passes, and the full suite passed 1867 of 1867.

## The verdict note on exit test 2

Disposition: full merit. D-605 names a run at the head of the PR.

Correction: the agent ran the bot session with `--frame-log` on the Deck over SSH at `e4d3543`, after the owner said the Deck was ready (D-606). The run used X11, as a player session does (F-147). It has 3877 frames, with p50 11111 µs and p99 11667 µs. After the first 10 s, no frame is over 11111 µs, the 90 frames per second of D-295. In the first 10 s, 67 frames were over, and the worst was 27270 µs after the first second. Each earlier run had the same pattern, on `main` too. The later commits of this response change documents alone, so the Game and the content of the run are those of the PR head.

## A finding outside the review: F-147

In some Deck runs, Godot did not exit after the end line, and a timeout ended it. The owner asked for a fix now. A probe ran the session under gdb on the Deck until a run hung, then read each thread. The main thread waits in `pthread_join`, and the thread "Wayland Events" waits in `poll`. Over SSH, X11 refused the session with no `XAUTHORITY`, and Godot fell back to Wayland.

- On the Wayland fallback, 5 of 14 runs hung: at PR-90 heads, and at `main` `60a23ec` with the same stack.
- With `XAUTHORITY` set to the Xwayland auth file, 0 of 9 runs hung.

The hang is in the Wayland shutdown of Godot 4.7.2, and it is older than PR-90. A player session on the Deck has the auth file, so Godot uses X11 there. `docs/runbooks/commands.md` now holds the Deck run procedure with `XAUTHORITY`, and `AGENTS.md` names it. `docs/design.md` records F-147 as done in this PR.

## New ids

F-147. The decisions of this round: none.

## Final head

The correction `e4d3543` is the effective head. The documents commit of this response comes after it.
