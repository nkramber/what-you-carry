# PR-95 review response

Date: 2026-09-23

Author: Claude Code. Review: `docs/reviews/pr-95.md`, round 1 at `d13f73c`, verdict `Changes required`.

## P2-1: README and LICENSE overrides cannot pass

Disposition: full merit.

Evidence: at `d13f73c`, `ReviewGateRules.EligiblePaths` held `docs/`, `CLAUDE.md`, `AGENTS.md`, and `.claude/skills/`. `SkipPaths` held those four and `README.md` and `LICENSE` (D-475). A PR of the root `README.md` alone therefore had no effective head, so the review path failed (D-540). The label path then refused the same path as outside the eligible set.

Correction: the owner gave D-541 in session: the `review-override` label covers each path of the skip set. `WhatYouCarry.Tools/ReviewGate/ReviewGateRules.cs` now reads `CiSkipRules.IsDocument` for the label, and `EligiblePaths` and `IsEligible` are gone. The skip set, the effective head, and the eligible set read one list. D-190 carries the mark `Revised in part by D-541`, the eligible set only.

Regression check: `ReviewGateGitTests.ReviewGatePassesOnOverrideLabelForARootDocument` for `README.md` and `LICENSE`, and `ReviewGateRulesTests.ReviewGatePassesOnOverrideLabelForEachPathOfTheSkipSet`. On the old `ReviewGateRules.cs`, the four `README.md` and `LICENSE` cases failed (4 of 4). With the correction, the review gate and review command tests pass 124 of 124. `ReviewGateFailsOnOverrideLabelWithADocumentOutsideTheSkipSet` proves that `WhatYouCarry.Game/README.md` and `.github/pull_request_template.md` stay outside the set.

## The verdict notes

- Smoke: the record says that the three Smoke jobs skipped at `d13f73c`. Smoke run 35892442993 on `d13f73c` ran `smoke-linux-x64`, `smoke-windows-x64`, and `smoke-macos-arm64`, each for 45 to 64 seconds, and each ended `success`. The jobs skipped only on the metadata heads `b019737`, `7781d3b`, and `5a84e04`, as D-474 intends.
- Night gate: the failure is the record of D-538, which the next PR holds. D-537 keeps the bypass for that case. PR-79 does not change the night gate.

## New ids

D-541.

## Final head

The correction commit is the new effective head. The handoff entry of Session 228 names it.
