# Runbook: the ruleset of main

Status: procedure, written 2026-09-23 (D-522). Written in ASD-STE100.

The file `.github/rulesets/main.json` holds the ruleset of `main`. The live ruleset on GitHub matches the file. `RulesetTests` binds the file to the workflows. Each job that runs on each PR is a required check (except `ci-skip` and `evaluate`), and each required check is the name of one job.

## What the ruleset holds

- The branch `refs/heads/main`, with the enforcement `active`.
- Squash merges alone, and resolved conversations (D-522).
- No required approval, because one person owns the repository.
- No extra approval for a commit of an unlinked author, and no required reviewers. GitHub adds both fields with other values when the file omits them, and an extra approval blocks every auto-merge of a one-owner repository.
- 20 required checks, each from the GitHub Actions app (id 15368). The job name of each check is unique across the workflows (F-110).
- No rule for the newest `main` on the branch, because the PRs go one at a time.
- No bypass, so no merge button offers to skip the rules (D-535 supersedes D-520). A night gate deadlock needs the procedure below.
- No deletion of `main`, and no force push to it.

A skipped job reports success to a required check. The heavy jobs skip a documents head (D-474), so each required check reports on a documents head and on a code head. A job that never reports blocks every merge.

## Procedure: the first setup

Do these steps once, after the owner approves the setup (D-519). Each command changes a repository setting, so get the approval first.

1. Turn on auto-merge for the repository.
2. Create the ruleset from the file on `main`.
3. Compare the live ruleset with the file.

```
repo=$(gh repo view --json nameWithOwner --jq .nameWithOwner)
gh api --method PATCH "repos/$repo" -F allow_auto_merge=true --jq .allow_auto_merge
git fetch origin main
git show origin/main:.github/rulesets/main.json > /tmp/main-ruleset.json
gh api --method POST "repos/$repo/rulesets" --input /tmp/main-ruleset.json --jq '{id, name, enforcement}'
```

## Procedure: a change of the ruleset

1. Change `.github/rulesets/main.json` and the workflow jobs in one PR.
2. Run `RulesetTests`.
3. After the merge, get the owner approval.
4. Update the live ruleset from the file on `main`.
5. Compare the live ruleset with the file.

```
id=$(gh api "repos/$repo/rulesets" --jq '.[] | select(.name == "main") | .id')
gh api --method PUT "repos/$repo/rulesets/$id" --input /tmp/main-ruleset.json --jq '{id, name, enforcement}'
```

## Procedure: a night gate deadlock

A fix PR for a night failure cannot turn `night-gate` green, and the ruleset has no bypass (D-535). The owner does these steps for that one merge alone.

1. Open Settings, then Rules, then the ruleset `main`.
2. Set the enforcement to `Disabled`, and save.
3. Merge the fix PR.
4. Set the enforcement to `Active`, and save.
5. Compare the live ruleset with the file.

## Compare the live ruleset with the file

The API adds fields that the file does not hold, such as the id and the dates. Compare the parts that the file holds.

```
keys='{name, target, enforcement, conditions, bypass_actors, rules: (.rules | sort_by(.type))}'
gh api "repos/$repo/rulesets/$id" | jq -S "$keys" > /tmp/live.json
jq -S "$keys" /tmp/main-ruleset.json > /tmp/file.json
diff /tmp/file.json /tmp/live.json && echo "The live ruleset matches the file."
```

An empty diff proves the match. A field that GitHub adds with a default value shows in the diff. Put that field and its value in the file, so the next comparison is empty.
