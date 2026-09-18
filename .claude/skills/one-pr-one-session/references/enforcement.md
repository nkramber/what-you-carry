# The enforcement of each rule

The lifecycle of a PR has rules, and each rule has an enforcement (D-375, D-376). A machine enforces a rule, or the agent does, or the owner does. Some rules are not observable, and the table says so.

Read this table when a question asks who catches a break of a rule. Section 3.14 of `docs/design.md` names this file.

| Rule | Enforcement | Mechanism |
|---|---|---|
| The PR changes `docs/session-handoff.md` | Machine | `doc-gate` |
| The newest handoff entry names the PR branch | Machine | `doc-gate` |
| The documents matrix gives each category exactly one line, with a disposition and a reason of five words or more | Machine | `doc-gate` |
| Each matrix line agrees with the changed paths | Machine | `doc-gate` |
| The description and the newest handoff entry put no documents off to later work | Machine, by a fixed list of phrases | `doc-gate` |
| No PR title or branch names a merge record | Machine | `doc-gate` |
| The handoff and the review record do not move the effective head | Machine | `review-gate` metadata set (D-184), and a test that pins the handoff path in that set |
| The skill has valid front matter, stays under 7000 characters, and the agent files name its path | Machine | `dotnet test` shape tests |
| The agent files, each skill file, each skill reference file, the handoff, and its newest entry stay under a byte ceiling | Machine | `dotnet test` context budget tests (D-382, D-384) |
| The handoff keeps 10 entries, newest first, and each older entry moves to the archive with its text intact | Machine, when the session runs the command | `handoff-rotate` and its seed-loop test (D-379) |
| A session reads the newest handoff entry, looks up register ids in one command, and waits on checks with one command | Agent | The read order of `AGENTS.md`, this skill, and `docs/runbooks/session-context.md` (D-377, D-378, D-380) |
| A reviewer loads `pr-review`, and an author who answers findings loads `review-response` | Agent | The skill descriptions and `AGENTS.md` (D-381) |
| No live document cites a superseded decision as current | Machine | `ste-check` reference check (D-178) |
| A reason is true and specific | Agent and owner | The author writes it, and the cross-provider review checks it |
| The design doc, the registers, and the roadmap agree with the PR | Agent | The author, then the cross-provider review |
| A session starts clean and works on one PR | Agent and owner | The start gate of this skill. The owner starts a new session for each PR |
| A session starts no other PR after the hand-over, and it stops at the merge. It can answer the findings of its own PR after the hand-over | Agent and owner | The closing line of this skill. The owner starts the session of the next PR |
| The identity of a session, and whether a context came from a compaction or a fork | Not observable | The harness exposes no session id, and the repository defines none |

An agent rule and an owner rule have no machine that catches a break. A session that skips one costs the next session its time. The cross-provider review reads each agent rule of the PR in front of it (T-4).
