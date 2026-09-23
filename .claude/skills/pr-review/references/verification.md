# Verification

The `pr-review` skill names this file at step 5. It holds the checks that a review runs, and the rules for the evidence of a check.

## Verification

Run the focused checks that can falsify the changed behavior. Complete the applicable project gates.
Use the current build commands in `AGENTS.md`. Do not invent a successful command when no solution or tool exists.

- Read the tests as critically as the implementation.
- Verify that each bug fix has a regression test that fails on the old behavior (T-3).
- Use an isolated comparison when execution of the regression test against the base is practical.
- Otherwise, explain the causal reason the old behavior fails the assertion and state the execution limit.
- Check test assertions against the contract, not a copy of the implementation.
- Inspect seed coverage, state diversity, boundary cases, and failure context (D-66).
- Check test discovery, skipped tests, mocks, fixtures, and assertions that can pass without the intended behavior.
- Distinguish a passed check from a skipped, unavailable, failed, or author-reported check.
- Record the command, revision, environment, result, and relevant artifact for each required check.
- Verify CI results against the reviewed revision and configured test target.
- A newer push to a PR cancels the older runs of each workflow (D-356). CI on the tip then counts as evidence for the effective head when every later commit is a metadata commit (D-357).
- The heavy jobs of `ci.yml`, `smoke.yml`, `bit-identity.yml`, and `bots.yml` skip a head that changes documents alone (D-474, D-477). The log of the `ci-skip` job names the rule. A skip by rule 2 counts as evidence for the head, because the previous head passed with the same code. The `documents` job runs on each head (D-476).
- A review of a PR of documents alone runs no full suite. Run `ste-check`, `doc-gate`, and the `Documents` category (D-491, D-492).
- A repeat review runs no full suite when each path after the reviewed head lies in the skip set of D-475. The commit of the review record runs none (D-491).
- A self-hosted job that ends with the annotation "not acquired" gives no result for the code. The author re-runs the failed jobs of that run, and the re-run counts as CI for that head (D-358).
- Check the three-platform bit-identity result and required smoke and content tests (D-71, D-114).
- Check the applicable bot, seed, economy, asset, and human gates in the current roadmap.
- Confirm that a known night failure does not bypass the next merge gate (D-115).

Use the initial-check clause only as D-148 and G-19 permit.
Name the absent check and the PR that creates it. A PR that creates a check must pass it.
The clause does not excuse a failed existing check.

Do not repeat broad suites without a new change, failure, or unresolved risk.
Do not weaken a test or threshold to obtain a pass.
Absent required evidence blocks approval. Optional evidence gaps belong in the limitations.
