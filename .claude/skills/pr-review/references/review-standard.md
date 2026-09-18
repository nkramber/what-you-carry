# The review standard

The `pr-review` skill names this file at step 4. It holds the depth of the review and the project contracts that a review reads.

## Principal-engineer review standard

Build an independent account of the behavior before comparison with the author's explanation.
For each changed behavior, trace the input, state transition, output, side effects, and recovery path.
State the invariant that each boundary must preserve.

### Correctness and system effects

- Check normal use, boundary values, absent data, invalid data, repeated actions, and interrupted actions where applicable.
- Trace state ownership and lifetime across Core, Game, Tools, and Tests.
- Inspect initialization, cancellation, cleanup, restart, and replay when the change affects those paths.
- Check event order, resource disposal, integer bounds, and float edge cases where they affect the result.
- Inspect compatibility with current callers, content, saves, and exported builds.
- Check whether a local fix creates a defect in another consumer of the same contract.
- Verify each acceptance criterion against implementation and evidence.

Do not expand the review into an unrelated rewrite.
Distinguish defects introduced by the PR, defects it exposes, and independent pre-existing defects.
A pre-existing defect blocks this PR only when it prevents the changed behavior or a required gate.

### Project contracts

Apply each relevant row. Record why an area does not apply when its omission can mislead a reviewer.

| Area | Required examination |
|---|---|
| Core boundary | No engine dependency or gameplay input from Godot physics or navigation. Trace data flow, not only imports (G-1, G-3). |
| Determinism | Seed ownership, stable iteration and event order, DetMath use, one simulation thread, and 60 Hz intents. Inspect the pure worker boundary (D-69 to D-77). |
| Replay | Immutable initial loadout, tree, and amulet state. Format and simulation versions, content hash, and the specified mismatch path. Verify the version bump for Core behavior changes (D-151, G-20). |
| Persistence | Profile generation, atomic replacement, run pointer, checksummed frames, and one-time completion by run id. Trace failure at each write boundary (D-152). |
| Errors | Required context, visible failure, safe recovery, and assertions in shipped builds. An empty catch or silent fallback violates T-2 (D-112, D-113). |
| Content | JSON schemas at load and in tests. Absent fields report the file, field, and reason. Check identifier references and file name case (D-91, D-92). |
| Input and CI boundaries | Check size limits, file paths, and validation at affected external inputs. Inspect CI permissions, secret access, and execution of untrusted content when those boundaries change. |
| Gameplay | Player and enemy rules, gear loss, permanent amulet, basic kit, timer, and floor transitions where affected. Trace repeated runs as well as one run (D-2, D-30, D-34, D-140, D-153). |
| Economy | Matched policies, fixed seeds, initial state, time denominator, and statistical bounds follow D-154. A smaller payout alone does not prove a lower rate. |
| Presentation | Controller use, Deck 800p readability, animation time values, and asset QA. Headless tests do not establish visual quality or game feel (D-15, D-87, D-135). |
| Dependencies and cost | A decision justifies each dependency. Performance claims include a profile before the change and a measurement after it (G-16, G-17). |

Do not reintroduce an earlier contract that a later decision supersedes.
For example, D-152 supersedes the three-file save design in D-94.

### Design, maintainability, and documents

- Confirm one concern per PR and a clear reason for every changed subsystem (G-10).
- Check helper depth against D-110.
- Require two concrete uses before an abstraction (D-111).
- Prefer explicit ownership and visible control flow over hidden coupling.
- Explain the concrete maintenance cost of a design objection.
- Do not report personal style preferences as correctness defects.
- Check that design text, decisions, questions, code, and acceptance criteria agree.
- Check each roadmap prerequisite against the first gate that needs it.
- Distinguish proposed work, implemented work, measured behavior, and owner approval.
- Verify material external claims against dated primary sources.
- Check each line of the documents matrix in the PR description against the diff. Each reason must be true and specific (D-118, D-376).
- Confirm `AGENTS.md` and `CLAUDE.md` remain identical when either changes (D-122).
- Check attribution restrictions in commits, PR text, comments, and deliverables (D-137).

Documentation and skill PRs require the same provider independence and evidence discipline as code PRs.
For a skill change, examine its trigger, scope, instructions, references, and behavior on a realistic request.
Treat contradictory instructions and gates that cannot pass as defects.
