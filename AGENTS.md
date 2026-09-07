# What You Carry: agent instructions

`CLAUDE.md` and `AGENTS.md` are identical (D-122). Edit both together.

## First action

Read `docs/session-handoff.md` now, before any other file and before any tool call. It tells you the state of the build, what is in flight, and the next concrete action. Then read the rest of this file.

## Read order

1. `docs/session-handoff.md`: the state and the next action.
2. This file: the tenets and the rules.
3. `docs/design.md`: the design, the guardrails (section 6), and the roadmap (section 7).
4. `docs/decisions.md`: every owner decision, D-1 onward. Cite a D-# id when you apply one.
5. `docs/questions.md`: the open questions register, OQ-1 onward. File a new question there.
6. `docs/reviews/`: one review file per PR, plus audits and audit responses.
7. `docs/roadmaps/`: focused roadmaps, when they exist.
8. `docs/session-handoff-archive.md`: sessions older than the 10 in the handoff. Read it only when the handoff points to it.

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
- One session is one harness invocation, one PR, and one handoff entry (D-121, D-146).

## Session handoff

At the end of a session, fetch the remote and read `docs/session-handoff.md` again. Take the highest session number and add one (D-187). Then add a new entry at the top (D-146). Keep the 10 newest entries in that file. Move any older entry to the top of `docs/session-handoff-archive.md`. Set the author field to `Claude Code` or `Codex`. Commit the entry with the review record or the work it describes (D-182). Another provider can add an entry above yours while you work. Add your own entry, and never append to an older one. Each entry has six parts:

- What the session did, and why.
- The state of the build.
- What is in flight.
- Traps and gotchas.
- The questions that block progress.
- The next concrete action.

## Text rules

- All project skills live in `.claude/skills/` (D-131, D-155). Create every new project skill there.
- Read each required skill from `.claude/skills/<skill-name>/SKILL.md`, even if it is absent from the skill list.
- Every `.md`, skill, and agent file follows ASD-STE100 (D-139). Load the `ste-writing` skill before you write.
- Load the `design-doc-style` skill before you edit `docs/design.md` or a focused roadmap.
- One term per concept. The `ste-writing` skill lists the project terms.
- Document file names in `docs/` are lowercase (D-129).

## Code rules

- C# only. GDScript is banned. Tools are C# (D-64, D-65).
- `WhatYouCarry.Core` has no engine dependency (G-1).
- No `System.Math` transcendentals, no `Vector<T>`, no SIMD, no reflection in Core. Use `DetMath` (G-2).
- float in Core (D-70). The simulation runs on one thread at 60 Hz (D-72, D-73).
- Godot physics and navigation never feed the simulation (G-3).
- JSON for all content, validated by a schema. No `.tres` files (D-91, D-92).
- All strings that the player sees live in the string table (D-98).
- Nullable reference types on, warnings as errors (D-68).
- xUnit. Property tests are seed loops. Each failure names its seed (D-66).
- Every dependency needs a decision entry (G-16).
- Every optimization needs a profile before and a measurement after (G-17).

## Git rules

- Trunk is `main`. Work on a short branch. The owner squash-merges (D-126).
- Commit subjects use a conventional prefix: `feat`, `fix`, `docs`, `test`, `chore`.
- One concern per PR (G-10).

## Build and test commands

No solution exists yet. PR-1 creates it. Until then, there is nothing to build. After PR-1, this section lists the exact commands.

## PR gate

A PR merges only when every line holds:

- [ ] Tests written and green (T-3).
- [ ] No silent failure. Every error carries context (T-2).
- [ ] The three-platform bit-identity job is green (G-9).
- [ ] The lint tool and the STE checker pass (G-2, G-14).
- [ ] The other provider reviewed it, and `docs/reviews/pr-<number>.md` has the verdict `Ready for owner merge` for the effective head (T-4, D-101, D-179, D-181, D-185). A PR that changes no code is exempt when the owner adds the `review-override` label (D-188, D-190).
- [ ] The `review-gate` check is green. Grey means no review record yet. Red means the review does not approve this head (D-179, D-181, D-185).
- [ ] `docs/decisions.md` has every new decision.
- [ ] `docs/questions.md` has every new question.
- [ ] `docs/design.md` matches intent.
- [ ] Any check that does not exist yet is named with the PR that creates it (D-148, G-19).
- [ ] `docs/session-handoff.md` is current.
- [ ] For each document not changed, the PR says "no change needed because ...".
- [ ] No attribution anywhere (T-6).
