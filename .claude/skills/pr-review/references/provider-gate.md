# The provider gate

The `pr-review` skill names this file at step 1. The gate runs before the substantive review and before any approval (T-4, D-101).

## Mandatory provider gate

**The reviewer MUST NOT come from the provider that wrote the PR.**
This requirement applies before the substantive review starts and before any approval (T-4, D-101).

| Provider that wrote the PR | Required reviewer |
|---|---|
| Claude Code, Anthropic | Codex, OpenAI |
| Codex, OpenAI | Claude Code, Anthropic |

A different model, account, session, or subagent from the same provider does not qualify.
A prompt that assigns the other provider's name does not change the actual provider.
Author self-checks and automated tests do not satisfy this gate.

Substantive changes alter code, data, configuration, requirements, or executable instructions.
Review findings and test reports alone do not make the reviewer a PR author.

1. Identify the actual reviewer provider from the active environment.
2. Identify every provider that contributed substantive changes or fixes to this PR.
3. Verify authorship from the owner's statement or the relevant handoff and review records.
4. Match each source to this PR and its revision.
5. Record the providers, source, and eligibility result in the review file.

The newest handoff entry can describe a review rather than authorship. A Git account alone does not identify the provider.
Do not infer authorship from prose style, commit email, or a branch name.

**Stop with `Blocked` if the providers match, authorship is unknown, or the evidence conflicts.**
State the fact or the eligible reviewer that the review needs. Ask the owner to supply that fact or start the opposite-provider session.
Do not perform a substitute review with another model from the same provider.

If both providers wrote substantive changes in the PR, neither qualifies for the whole PR.
Record the conflict and request an owner decision about how to separate the changes.
Do not approve through reciprocal review of selected hunks.
