# PR-10 response

Date: 2026-09-08

Answers `docs/reviews/pr-10.md`, verdict `Changes required` at head `3ea5f23`.

## P2-1: The sentence splitter does not end every sentence at a colon

Disposition: partial merit.

Evidence: the whitespace condition is deliberate, and the finding's correction breaks real text. `.claude/skills/ste-writing/SKILL.md:10` holds the bare URL `https://www.asd-ste100.org/` in prose. A split at every colon cuts that line after `https:` and cuts a time such as `10:30` or a ratio such as `16:9`. The period has the same condition for the same reason: `4.7.2` is one word. Rule 8.4 says that a colon counts as the end of a sentence in a vertical list, and it says nothing about a colon inside a word.

The part with merit: the roadmap result paragraph and the `ste-writing` skill said that a colon ends a sentence "everywhere", and the code does not do that. The wording over-claimed. Both now say that a colon that a space or the line end follows ends a sentence, in any text, and that a colon inside a word does not. The doc comment on `SentenceText` says the same.

Regression check: `ColonInsideAWordDoesNotEndASentence` asserts that a URL, a time, and a ratio stay in one sentence, that `Options: the value.` gives two sentences, and that `Options:the value.` gives one. It passes.

## P2-2: Nested parentheses do not count as one opaque word

Disposition: full merit.

Reproduced: `Read (the (short) name) now.` gave the words `Read`, `(the (short)`, `name)`, and `now.`, four words instead of three, and the grammar rules could read `name)`.

Correction: `MaskSpans` now finds the close character through `FindSpanEnd`, which counts depth when the open and close characters differ. Backticks and quotes have one character for both ends, so they cannot nest, and the depth is one. An unclosed span stays plain text, as before. Commit: the commit that holds this file.

Regression check: `NestedParenthesesAreOneOpaqueWord` asserts three words for the example, asserts that the words are `Read`, `(the (short) name)`, and `now.`, asserts no passive finding for `The name (it was written (by hand) once) is short.`, and asserts five words for an unclosed span. It fails on the old code with four words. It passes.

## P2-3: The command silently accepts trailing arguments

Disposition: full merit.

Reproduced: `ste-check --root . trailing-argument` and `ste-check --root . --unknown` both returned 0 at `3ea5f23`. That is a silent failure (T-2).

Correction: the argument loop in `SteCheckCommand.Run` now consumes `--root <value>` pairs only. Any other argument, a second option, a trailing positional, or `--root` without a value prints `Unexpected argument '<argument>'` with the one accepted option, and exits 2. Commit: the commit that holds this file.

Regression check: `SteCheckCommandExitsOneOnAFindingAndZeroWhenClean` now also asserts exit 2 for a trailing positional argument, for an extra option, and for `--root` without a value. The first two assertions fail on the old code with exit 0. The test passes, and `ste-check --root . trailing-argument` exits 2 with the message.

## New ids

None. No decision and no question came from this review.

## Verification

- `dotnet build WhatYouCarry.slnx -m:1`: 0 warnings, 0 errors.
- `dotnet test WhatYouCarry.slnx --no-build -m:1`: 63 tests, 0 failures.
- `dotnet run --project WhatYouCarry.Tools/WhatYouCarry.Tools.csproj -- ste-check --root .`: 0 findings in 15 files.

## Final PR head

The corrections, this file, and the session 23 handoff entry are in one commit (D-182), so that commit is the effective head. Its hash is in the session 23 handoff. The review commit `29b1066` was on the local checkout only, and this push carries it to the branch (D-183).
