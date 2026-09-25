# Runbook: the ruleset of main

Status: procedure, written 2026-09-23 (D-522). Written in ASD-STE100.

The file `.github/rulesets/main.json` holds the ruleset of `main`. The live ruleset on GitHub matches the file. `RulesetTests` binds the file to the workflows. Each job that runs on each PR is a required check (except `ci-skip` and `evaluate`). Each required check but `review-gate` is the name of one job. The `evaluate` job posts the `review-gate` check run (D-181).

## What the ruleset holds

- The branch `refs/heads/main`, with the enforcement `active`.
- Squash merges alone, and resolved conversations (D-522).
- No required approval, because one person owns the repository.
- No extra approval for a commit of an unlinked author, and no required reviewers. GitHub adds both fields with other values when the file omits them, and an extra approval blocks every auto-merge of a one-owner repository.
- 20 required checks, each from the GitHub Actions app (id 15368). The job name of each check is unique across the workflows (F-110).
- No rule for the newest `main` on the branch, because the PRs go one at a time.
- One bypass: the repository admin role, through a pull request merge alone (D-520). It clears a night gate deadlock.
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
ruleset=$(mktemp "${TMPDIR:-/tmp}/main-ruleset.XXXXXX")
git show origin/main:.github/rulesets/main.json > "$ruleset"
gh api --method POST "repos/$repo/rulesets" --input "$ruleset" --jq '{id, name, enforcement}'
```

Each procedure sets its own variables and writes its own temporary files with `mktemp`. A fixed path such as `/tmp/main-ruleset.json` can hold a file of another session, and the upload then sends that file (F-126).

## Procedure: a change of the ruleset

1. Change `.github/rulesets/main.json` and the workflow jobs in one PR.
2. Run `RulesetTests`.
3. After the merge, get the owner approval.
4. Update the live ruleset from the file on `main`.
5. Compare the live ruleset with the file.

```
repo=$(gh repo view --json nameWithOwner --jq .nameWithOwner)
git fetch origin main
ruleset=$(mktemp "${TMPDIR:-/tmp}/main-ruleset.XXXXXX")
git show origin/main:.github/rulesets/main.json > "$ruleset"
id=$(gh api "repos/$repo/rulesets" --jq '.[] | select(.name == "main") | .id')
gh api --method PUT "repos/$repo/rulesets/$id" --input "$ruleset" --jq '{id, name, enforcement}'
```

## Compare the live ruleset with the file

The API adds fields that the file does not hold, such as the id and the dates. Compare the parts that the file holds.

```
repo=$(gh repo view --json nameWithOwner --jq .nameWithOwner)
git fetch origin main
work=$(mktemp -d "${TMPDIR:-/tmp}/main-ruleset.XXXXXX")
git show origin/main:.github/rulesets/main.json > "$work/main-ruleset.json"
id=$(gh api "repos/$repo/rulesets" --jq '.[] | select(.name == "main") | .id')
keys='{name, target, enforcement, conditions, bypass_actors, rules: (.rules | sort_by(.type))}'
gh api "repos/$repo/rulesets/$id" | jq -S "$keys" > "$work/live.json"
jq -S "$keys" "$work/main-ruleset.json" > "$work/file.json"
diff "$work/file.json" "$work/live.json" && echo "The live ruleset matches the file."
```

An empty diff proves the match. A field that GitHub adds with a default value shows in the diff. Put that field and its value in the file, so the next comparison is empty.
