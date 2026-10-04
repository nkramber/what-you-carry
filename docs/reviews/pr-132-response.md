# PR-132 review response

Date: 2026-10-03

Author: Claude Code. Review: `docs/reviews/pr-132.md`, round 1 at `33f81fe`, verdict `Changes required`.

## P2-1: Security updates do not use cross-directory grouping

Disposition: full merit.

Evidence: the GitHub Dependabot options reference states that `group-by` applies to version updates only. A group with no `applies-to` key applies to version updates alone. The group `each-action` at `33f81fe` has no `applies-to` key. A security update for an action in `/` and in `/.github/actions/*` therefore comes as one PR for each directory. The merge of one PR alone then leaves the action on two pins, as #115 did. GitHub also states that Dependabot groups one ecosystem across the directories of a `directories` configuration when grouping rules apply.

Correction: the owner chose the security group in session (D-776). `.github/dependabot.yml` adds the group `security` with `applies-to: security-updates` and the pattern `"*"`. One security PR then moves each action with an advisory in every directory. D-774 carries the mark `Revised in part by D-776`, the security updates only. The group `each-action` of the version updates stands.

Regression check: `ActionDecisionTests.DependabotGroupsSecurityUpdatesInEveryDirectory` reads the group. On the file of `33f81fe` it failed, 1 of 1. With the correction, `ActionDecisionTests` passed 11 of 11, and the full suite passed 2,191 tests.

## New ids

D-776.

## Final head

The correction commit is the new effective head. The handoff entry of Session 331 names the round.

## The merge of main after PR #133

PR #133 merged first and took D-774 to D-776 on `main`. The merge of `main` into this branch moves the ids of this branch to the next free ids, as D-772 orders for this branch. D-774 is now D-777, D-775 is now D-778, and D-776 is now D-779. The text of each decision stays the same. The sections above keep the ids of round 1. The handoff entries of Sessions 331 to 333 of this branch are now Sessions 338 to 340, with the new ids. `.github/dependabot.yml` and `ActionDecisionTests` cite the new ids in their remarks. The change moves the effective head, so the review reads the new head.
