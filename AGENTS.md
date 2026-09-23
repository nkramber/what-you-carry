# What You Carry: agent instructions

`CLAUDE.md` and `AGENTS.md` are identical (D-122). Edit both together.

## First action

Read the newest entry of `docs/session-handoff.md` now, before any other file. Print that entry alone with `awk '/^## Session /{n++} n==1' docs/session-handoff.md`. Then read the rest of this file.

## Read order

1. `docs/session-handoff.md`: the newest entry, and the newest entry that names your branch. Read an older entry only when one of the two points to it (D-377).
2. This file: the tenets and the rules.
3. `docs/design.md`: the guardrails (section 6) in full. Find another section with `grep -n '^##' docs/design.md`.
4. `docs/decisions.md`: every owner decision, D-1 onward. Cite a D-# id when you apply one.
5. `docs/questions.md`: the open questions register, OQ-1 onward. File a new question there.
6. `docs/reviews/`: one review file per PR, plus audits and audit responses.
7. `docs/roadmaps/`: the entry of your PR and its exit tests.
8. `docs/session-handoff-archive.md`: older sessions. Read it only when the handoff points to it.

Never read `docs/decisions.md` or `docs/questions.md` in full. Look up the ids of the task in one early command (D-378). Replace the example numbers in `d` and `q` with every D-# and OQ-# number of the task:

```
d='146|375'; q='9|44'
grep -n -E "^\| D-($d) \|" docs/decisions.md
grep -n -E "\bD-($d)\b" docs/decisions.md | grep -E 'Revis|Supersed' | cut -c1-160
grep -n -E "^[0-9]+\. \*\*OQ-($q)\." docs/questions.md
```

The third line finds each revision of those ids (D-186).

## Tenets

The tenets are the constitution. When a tenet conflicts with speed or convenience, the tenet wins. When two tenets conflict, the earlier one in this order wins (D-119): T-5, T-2, T-3, T-4, T-1. T-6 is absolute.

- **T-1. Readable, simple, not wasteful.** Explicit over implicit. A fresh model must understand a function from the function and its helper signatures. Helpers go one level deep (D-110). Two concrete cases before any abstraction (D-111). No clever one-liners. Tune only on measurement (D-109).
- **T-2. Zero silent failures.** No empty catch blocks. An absent value is an error, never a zero. Every error carries its context (D-113). Assertions stay on in shipped builds (D-112).
- **T-3. Tests cover everything.** No merge without tests. A bug fix ships with a regression test that fails on the old code.
- **T-4. Cross-provider review before merge.** The provider that wrote the code does not review it. The review file in `docs/reviews/` records the findings (D-101). A PR that changes no code merges without a review when the owner adds the `review-override` label (D-188, D-190).
- **T-5. Document everything.** Continuity is the first duty. Each session adds its entry at the top of `docs/session-handoff.md` (D-146). The other documents update when intent, a decision, or a plan changes (D-118).
- **T-6. No attribution.** No code, game text, commit, PR description, or GitHub comment names an agent, harness, or model as the source of work (D-137). Two places are exempt: the author field in `docs/session-handoff.md`, and the files in `docs/reviews/`.

## Attribution rule in practice

- Add no co-author trailer and no "generated with" line to any commit or PR.
- Write commits, PRs, comments, code, and docs in an impersonal voice.
- In `docs/session-handoff.md`, set the author field to exactly one value: `Claude Code` or `Codex`. The reviewer uses it to confirm the other provider reviews.

## How to work with the owner

- Ask the moment you have a question (D-138). Use `AskUserQuestion` in small batches. Give the options, the reasons, and a recommendation.
- Push back when a request rests on a wrong premise. Give the evidence.
- When two owner statements conflict, say so and quote both.
- Every open question belongs to the owner (D-124). Do not pick a default. File the question in `docs/questions.md` and stop.
- Record each answer in `docs/decisions.md` with the next D-# id and the date. Never renumber.
- Mark a change to an earlier decision in its `Effect` column (D-186). Use `Superseded by D-N` when the whole answer changes. Use `Revised in part by D-N` when one part changes, and name the part that changed and the parts that stand.
- A citation of a superseded decision must name the superseding decision. A decision revised in part stays citable.
- A reviewer loads `pr-review`. An author who answers review findings loads `review-response` (D-381).
- On the three-strike stop of `make codex-review`, turn off auto-merge and ask the owner (D-513).
- One session is one harness invocation, one PR, and one role (D-121, D-375). The author starts the review with `make codex-review` (D-511). Load `.claude/skills/one-pr-one-session/SKILL.md` before all PR work: implementation, a new or continued PR, a review, an answer to findings, or the documents of a PR. No PR exists only to record an earlier PR. Each PR has its own handoff entry (D-146).

## Session handoff

At the end of a session, fetch the remote. Print the highest session number with `grep -m1 '^## Session ' docs/session-handoff.md`, and add one (D-187, D-377). Add a new entry at the top with one edit (D-146). Then run `handoff-rotate` (D-379). It moves each entry after the tenth to the archive top. It also puts an entry that sits under an older one back in its place, and it names that entry (D-406). Commit the entry with the review record or the work it describes (D-182). Push, then fetch, and check that the status shows no `[ahead N]` (D-199). Another provider can add an entry above yours while you work. Add your own entry, and never append to an older one. The session line of the entry names the PR branch in the form Branch `<branch>` (D-376). Each entry has six parts:

- What the session did, and why.
- The state of the build, with the remote head (D-199).
- What is in flight.
- Traps and gotchas.
- The questions that block progress.
- The next concrete action.

## Text rules

- All project skills live in `.claude/skills/` (D-131, D-155). Create each new skill there. Read each required skill from `.claude/skills/<skill-name>/SKILL.md`, even if the skill list does not name it.
- A skill can hold reference files under `.claude/skills/<skill-name>/references/`. The skill file names each one and the step that needs it. Load a reference file at that step, and not before (D-383, D-385).
- Every `.md`, skill, and agent file follows ASD-STE100 (D-139). Load the `ste-writing` skill before you write.
- Load the `design-doc-style` skill before you edit `docs/design.md` or a focused roadmap.
- One term per concept. The `ste-writing` skill lists the project terms.
- Document file names in `docs/` are lowercase (D-129).
- A test caps the bytes of the agent files, the handoff, and every `.md` file under `.claude/skills/`, a reference file included (D-382, D-384). The `ste-writing` skill states each ceiling.

## Code rules

The `csharp-conventions` skill holds the code rules. It covers the language, the build, the project boundaries, and determinism. It also covers the Core allowlist procedure, the errors, the content, the strings, the tests, and the shape of the code. Load `.claude/skills/csharp-conventions/SKILL.md` before you write or review C#.

## Git rules

- Trunk is `main`. Work on a short branch. A PR squash-merges by auto-merge or by the owner. First give the owner the merge summary of D-533, and get the merge confirmation (D-126, D-516, D-524).
- Commit subjects use a conventional prefix: `feat`, `fix`, `docs`, `test`, `chore`.
- After a push, wait on the checks with the one command of the `one-pr-one-session` skill, and never poll (D-380).
- One concern per PR (G-10).
- A newer push to a PR cancels the older runs of each workflow for that PR (D-356). CI on the tip counts for the effective head when every later commit is a metadata commit (D-357).
- When a self-hosted job of a PR run ends with the annotation "not acquired", re-run the failed jobs of that run. The re-run counts as CI for that head (D-358).

## Automated review pass

An automated reviewer, gitar, comments on every PR after a push (D-250). The author answers every comment before the hand-over to the other provider, or before the override request on a documentation PR.

- Load the `gitar-review` skill after each push. It holds the author procedure, the proof that a review is current, and the commands (D-374).
- When the pass ends, run `make codex-review PR=<n>` in the background, or ask for the override (D-511, D-517).
- The reviewing provider reads the PR comments into its review and never addresses gitar (`pr-review`).
- A reply names no provider, harness, or model as the source of work (T-6).

## Build and test commands

The build needs the SDK version in `global.json`. Run each command from the checkout root. A command in the form `tools <command>` runs `dotnet run --project WhatYouCarry.Tools/WhatYouCarry.Tools.csproj -- <command>`.

- Build: `dotnet build WhatYouCarry.slnx`
- Test: `dotnet test WhatYouCarry.slnx --no-build`
- STE check, the reference check, and the session number check: `tools ste-check --root .`
- Handoff rotation: `tools handoff-rotate --root .`
- Documentation gate, local run: `tools doc-gate --root . --base origin/main --head HEAD --body <file> --title "<title>" --branch <branch>`
- Determinism and string lint: `tools det-lint --root .`
- Asset QA: `tools asset-qa --root .`
- Texture generator: `tools texture-gen --root .`
- Sounds: `make sounds` renders them, and `make analyze SOUND=<name>` analyses one reference.
- Bit identity: `tools bit-identity`
- Review gate, local run: `tools review-gate --input request.json --output check-run.json`
- Cross-provider review: `make codex-review PR=<n>`. Make prints its exit code as `Error <code>` (D-511).
- Godot build check: `/Applications/Godot_mono.app/Contents/MacOS/Godot --headless --editor --path WhatYouCarry.Game --build-solutions --quit`
- Smoke session, local run: `/Applications/Godot_mono.app/Contents/MacOS/Godot --headless --path WhatYouCarry.Game --fixed-fps 60 -- --smoke`
- Test exit session, a headless smoke session that presses Escape or Start at a tick: `/Applications/Godot_mono.app/Contents/MacOS/Godot --headless --path WhatYouCarry.Game --fixed-fps 60 -- --smoke --press escape 100`. The other name is `start`.
- Play session: `make play` builds and opens it borderless fullscreen at the display resolution, with the mouse captured (D-310). Escape or the Start button ends it (D-311). `make windowed` opens a window.
- Bot session with a frame log, for M-3: `/Applications/Godot_mono.app/Contents/MacOS/Godot --path WhatYouCarry.Game -- --bot --frame-log frames.txt`
- Transition test, PR-18 exit test 6 on the Deck (D-428, D-435): `/Applications/Godot_mono.app/Contents/MacOS/Godot --path WhatYouCarry.Game -- --bot --frame-log frames.txt --transitions 10`. On the Deck, use the Linux Godot .NET binary of D-294 in place of that path.
- Contact sheet, a local run with a window: `/Applications/Godot_mono.app/Contents/MacOS/Godot --path WhatYouCarry.Game -- --contact-sheet sheet.png`
- HUD shot, the Deck frame of the HUD fixture, with a window (D-133): `/Applications/Godot_mono.app/Contents/MacOS/Godot --path WhatYouCarry.Game -- --hud-shot hud.png`

The Game layer checks the user arguments after `--` at boot. A bad argument ends the boot with exit code 1, and the error line names it (D-313, D-317). The contact sheet and the HUD shot take no other flag, `--smoke` and `--bot` exclude each other, and `--transitions` needs `--bot` and `--frame-log`.

`Godot` is not on the command path of this machine, so use the full path above. `det-lint` reports one count for Core and one for Game (D-222). The PR gate names what `det-lint` and `asset-qa` read. A change of documents alone (the skip set of D-475) runs no full suite, for the author, a review, or a handoff. It runs `ste-check`, `doc-gate`, and `dotnet test WhatYouCarry.slnx --filter Category=Documents` (D-491, D-492). A change with any other path runs the full suite (D-493). The `csharp-conventions` skill holds the Smoke and CI filters.

`texture-gen` writes `content/textures/atlas.png` and `layout.json` from the palette, the recipes, and the paint files (D-305, D-505). Commit both after each change, because a test compares them with the output. `audio-synth` writes a WAV file next to each sound file under `content/audio/sfx/`, and a test compares each one too (D-453). `audio-analyze` writes the band levels of a reference into a spectral layer (D-464). The `Makefile` holds the commands of this list: run `make`. The contact sheet needs a window, so it never runs in CI, and a headless run exits 1 (D-306).

Each project has one directory at the root, beside the solution file, and `project.godot` sits in `WhatYouCarry.Game/`. Each project file names its target framework, because the Godot editor writes `net8.0` into a project file that has none.

## PR gate

A PR merges only when every line holds:

- [ ] Tests written and green (T-3). A PR of documents alone needs the checks of D-491 green in place of the suite (D-494).
- [ ] No silent failure. Every error carries context (T-2).
- [ ] The three-platform bit-identity job is green (G-9).
- [ ] The `det-lint` job is green. It reads Core for the determinism rules and Game for the string rule (G-2, G-8, G-21).
- [ ] The `asset-qa` job is green: the clip check, the overlay check, and the file case check over every model, overlay, and animation (D-135, D-300, D-301, D-302).
- [ ] The `ste-check` job is green (G-14).
- [ ] The `night-gate` job is green: a success record from a night inside 48 hours, at a commit on the base branch (D-115, D-177, D-274, D-275).
- [ ] The `smoke` job is green on all three platforms: the headless smoke session of the Game layer, with the pinned Godot binary (D-114, D-149).
- [ ] The automated pass of gitar approved the head, or every comment of the pass has its answer (D-250).
- [ ] The other provider reviewed it through `make codex-review`, and `docs/reviews/pr-<number>.md` has the verdict `Ready for owner merge` for the effective head (T-4, D-101, D-179, D-181, D-185, D-534). A PR that changes no code is exempt when the owner adds the `review-override` label (D-188, D-190).
- [ ] The `review-gate` check run is green. Red means no review record, or a review that does not approve this head (D-179, D-181, D-185, D-521).
- [ ] `docs/decisions.md` has every new decision.
- [ ] `docs/questions.md` has every new question.
- [ ] `docs/design.md` matches intent.
- [ ] Each check that does not exist yet has a line that names the PR that creates it (D-148, G-19).
- [ ] `docs/session-handoff.md` is current.
- [ ] The `doc-gate` job is green: the handoff entry names this branch, and the documents matrix gives each category a disposition and a reason (D-375, D-376).
- [ ] No review thread stays open, and the ruleset of `main` holds (D-522).
- [ ] No attribution anywhere (T-6).
