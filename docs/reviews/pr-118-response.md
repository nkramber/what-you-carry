# PR-118 response

Date: 2026-09-27

Author: Claude Code. Review: `docs/reviews/pr-118.md`, verdict `Blocked` at the effective head `ce02802`.

## The block: the raw Deck frame logs of exit test 2

Disposition: full merit. Exit test 2 is a measurement, and the review could not read the logs that the handoff reported. No earlier PR committed a raw frame log, so the record gave no path to them.

Correction: the raw logs are now in `docs/reviews/pr-118-deck/`. That directory is in the metadata set of D-184, so the effective head stays `ce02802`. Each frame log holds one frame per line, in microseconds of the real frame time (D-635). The agent ran each log on the Deck over SSH, after the owner said that the Deck was ready (D-606), with the bot command of `docs/runbooks/commands.md` and the Deck steps of that file.

| File | Commit | What it shows |
|---|---|---|
| `exit-test-2-ce02802-run-1.txt` to `run-3.txt` | `ce02802` | Exit test 2 at the effective head, with no patch |
| `main-3fdefef-run-1.txt` and `run-2.txt` | `3fdefef` | The base, for the comparison of OQ-209 |
| `msaa-4x-c8167ce-run-*.txt`, `msaa-2x-*`, `msaa-off-*` | `c8167ce` | The three modes of D-684. An `override.cfg` on the Deck set the mode of 2x and off. |
| `empty-scene-probe.txt` | none | A throwaway scene of one cube, with the same Godot binary and session (F-190) |
| `trace-0785aad-with-timing-patch.txt` | `0785aad` | The trace before the shared meshes: the rebuild took 16695 µs (F-192) |
| `trace-ce02802-with-timing-patch.txt` | `ce02802` | The trace after the shared meshes: the rebuild took 4278 µs |

A trace line `FT` holds the tick, the frame µs, the tick µs, the upload µs, the GC pause µs, the gen 0, 1, and 2 counts, the dig flag, the render CPU and GPU µs, the process µs, and then the bot µs and the step µs. A line `SWAP` holds the time of the chunk swap or of the enemy rebuild. A timing patch that never entered the branch wrote both files.

## The method of exit test 2 (D-683)

1. Add the frame times in order. The time of a frame is the sum up to and including it.
2. Drop each frame at 10 seconds or earlier.
3. List each frame over 16700 µs.
4. The stall of F-190 comes every 2.245 seconds. Take the phase as the median of the frame time modulo 2.245 over the listed frames of 20500 µs or less. A listed frame within 0.15 seconds of that grid is a stall.
5. Each other listed frame must be the transition, and the transition must be under 22222 µs. The bot summary line of the session names the transition as `transitionMicrosMax`.

The result for the three runs at `ce02802`:

| Run | Frames after 10 s | p50 µs | p99 µs | Frames over 16700 µs | On the F-190 grid | Largest on the grid | Other frames |
|---|---|---|---|---|---|---|---|
| 1 | 2991 | 11111 | 11771 | 16 | 15 | 19758 | 20050 at 42.24 s, the transition |
| 2 | 2990 | 11112 | 11691 | 16 | 15 | 19583 | 20310 at 42.24 s, the transition |
| 3 | 2990 | 11106 | 11898 | 16 | 15 | 19464 | 19923 at 42.23 s, the transition |

The same method on `main-3fdefef-run-1.txt` gives p99 13038 µs and a transition of 33512 µs. On `empty-scene-probe.txt` it gives 13 frames over 16700 µs, each on the grid, and no other frame.

## Regression checks

- The documents of this answer change the metadata set alone, so the effective head stays `ce02802`, and no code check runs again (D-184, D-534).
- `ste-check`, `doc-gate`, and the `Documents` category ran on this commit (D-491).

## New ids

None. The final PR head is the commit of this file.
