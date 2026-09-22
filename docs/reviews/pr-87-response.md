# PR-87 review response

Date: 2026-09-22

## Identity

- PR: 87
- Review record: `docs/reviews/pr-87.md`, verdict `Changes required` for head `06cd3b3e9c9bbb6144c26999c7e8db0b40cf537b`
- Author: Claude Code
- Reviewer: Codex

## State of the checkout

`git fetch origin` and `git status --short --branch` showed no commit ahead of the remote, so the review commit
`66cfa71` needed no push (F-59).

## P2-1: The analysis command writes outside the sound directory

Disposition: full merit.

The trigger reproduces. A scratch checkout with `pwn.wav` at its root took `audio-analyze --root <checkout>
--sound ../../../pwn`, exited 0, and wrote `pwn.json` at the checkout root, outside `content/audio/sfx/`.

Correction: `AudioAnalyzeCommand.IsOneName` takes the sound name before the command builds a path. A name passes
when it is one file name: not empty, not `.` or `..`, with no `/`, `\`, or `:`, with no character that the
platform bars from a file name, and equal to its own file name. The command prints the reason and gives exit code
2, which is the code of a bad command line. The whole change is 13 lines in
`WhatYouCarry.Tools/AudioSynth/AudioAnalyzeCommand.cs` (T-2).

Regression check: `TheAnalysisTakesOneFileName` runs the command in a temporary checkout for `../../../pwn`,
`../escape`, `sub/footstep`, `..`, and the empty name. It asserts exit code 2, that no file lies at the checkout
root, and that the sound directory stays empty. The five cases fail on the code before the correction.

## P2-2: An overflowing WAV chunk length escapes validation

Disposition: full merit.

The trigger reproduces. A 20-byte RIFF file whose `fmt ` chunk names the length 2147483640 raised an unhandled
`ArgumentOutOfRangeException` from `BitConverter.ToInt16`, with no file and no chunk in the message. The sum
`body + length` passes the top of an int and turns negative, so the guard of the reader accepted it.

Correction: the guard takes the remaining count off the length instead of adding the length to the offset:
`length > bytes.Length - body`. The subtraction cannot pass the range, because `body` never passes
`bytes.Length`. The change is one line and one comment in `WhatYouCarry.Tools/AudioSynth/WavReader.cs` (D-113,
T-2).

Regression check: `AWavChunkPastTheEndFails` reads a short RIFF file whose chunk length is 2147483640, the
largest int, and 40. Each one raises a `ContextException` that names the file, the chunk, and the reason. The
three cases fail on the code before the correction.

## Evidence of the corrections

- Both triggers again, after the correction: the traversal name gives exit code 2 and writes no file, and the
  malformed file gives `The reference file '...' is not valid: the chunk 'fmt ' has a length past the end of the
  file.` with the file and the chunk in the context.
- The six new cases fail on the code before the correction, and pass after it.
- The audio tests: 48 passed, 0 failed.
- `ste-check --root .`: 0 findings in 35 files.

## New ids

None. The corrections restore the contracts of T-2 and D-113, and they need no decision.

## Out of scope

The review agrees that F-107 and F-108, the enemy diagonal and the enemy ramp walk, belong to a later gameplay PR.
