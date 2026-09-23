# Runbook: the commands of a session

Status: procedure, written 2026-09-18 (D-383). Written in ASD-STE100.

A session pays for each byte that it reads and for each model call that it makes. This runbook holds the commands that keep both counts low. The skills name a section of this file at the step that needs it. The commands read the repository from the working directory.

## Targeted reads

Read the newest handoff entry first, and read that entry alone (D-377).

```
awk '/^## Session /{n++} n==1' docs/session-handoff.md
```

Read the newest entry that names your branch, when your branch is not new.

```
awk -v b='<branch>' '/^## Session /{n++} n>0 && $0 ~ b {print n; exit}' docs/session-handoff.md
```

Never read `docs/decisions.md` or `docs/questions.md` in full (D-378). Look up the ids of the task in one command. Replace the example numbers with every D-# and OQ-# number of the task.

```
d='146|375'; q='9|44'
grep -n -E "^\| D-($d) \|" docs/decisions.md
grep -n -E "\bD-($d)\b" docs/decisions.md | grep -E 'Revis|Supersed' | cut -c1-160
grep -n -E "^[0-9]+\. \*\*OQ-($q)\." docs/questions.md
```

The second line finds each revision of those ids (D-186). A `Superseded by D-N` mark replaces the whole answer. A `Revised in part by D-N` mark changes one part, and the rest of that decision stays current.

Find a section of the design doc, and read that section alone.

```
grep -n '^##' docs/design.md
sed -n '<start>,<end>p' docs/design.md
```

Find the roadmap entry of your PR, and read that entry alone.

```
grep -rn '^### PR-<number>:' docs/roadmaps/
```

## The commit command

A commit of a record holds the record and the handoff entry together (D-182). Write the message in an impersonal voice, and name no provider, harness, or model (T-6, D-137).

```
git add <paths> docs/session-handoff.md
git commit -m '<prefix>: <subject>'
git push origin <branch>
```

Run the session end gate after the push (D-199). The evidence comes from the remote, and not from the local checkout.

```
git fetch origin
git status --short --branch
git rev-parse HEAD
gh pr view <number> --json headRefOid --jq .headRefOid
```

The status line shows no `[ahead N]`, and the two hashes are the same. A shell chain can continue after a failed command, so read the output of each line (F-96).

## Wait for the checks

Each status poll costs a model call over the whole context. Wait with one command after each push (D-380).

```
gh pr checks <number> --watch --interval 60 > /dev/null 2>&1; gh pr checks <number>
```

Run no other status command while the wait runs. Read the result one time, when the command ends. The command waits for every check, so do not add `--fail-fast`. The `evaluate` check fails until a review record exists (D-251).

- Run the command in the background when the harness permits that.
- A time limit of the harness can stop the wait. Start the same command again.
- The result names each failed job. For a job that ends "not acquired", apply D-358, and then wait again with the same command.

## The review round

The author starts the cross-provider review with one command after the gitar pass (D-511). A round takes longer than the ten-minute limit of a tool call, so run it in the background and wait for the completion notice.

```
make codex-review PR=<number>
```

Run no status command while the round runs. The last lines of the output give the outcome, the verdict, the open finding ids, and the transcript path. The file `.claude/skills/one-pr-one-session/references/review-and-merge.md` gives the next step for each exit code.

## Wait for gitar

The `gitar-review` skill holds the procedure and the proof that a review is current (D-374). This section holds the two commands of the wait. Do the full push wait of three minutes after each push, and do it also when gitar paused the automatic reviews (D-160).

```
repo=$(gh repo view --json nameWithOwner --jq .nameWithOwner)
push=$(date +%s)
while [ $(( $(date +%s) - push )) -lt 180 ]; do sleep 15; done
h=$(gh pr view <number> --json headRefOid --jq .headRefOid)
gh api "repos/$repo/commits/$h/check-runs" \
  --jq '.check_runs[] | select(.app.slug == "gitar-bot") | "\(.status) \(.conclusion) \(.started_at)"'
```

Put each later wait in one shell loop that prints the final state alone. A tool that refuses a long command can run the loop in the background.

## The comment export

A PR carries two kinds of comment: the issue comments, which hold the dashboard of gitar, and the review threads, which hold each finding. Export both to one file, and then read the file. One export costs one model call, and a thread-by-thread read costs many.

```
repo=$(gh repo view --json nameWithOwner --jq .nameWithOwner)
owner=${repo%/*}; name=${repo#*/}; n=<number>
out=/tmp/pr-$n-comments.md

{
  echo "# Issue comments of PR $n"
  gh api --paginate "repos/$repo/issues/$n/comments" \
    --jq '.[] | "\n## \(.user.login) \(.created_at) edited \(.updated_at) id \(.id)\n\n\(.body)"'
  echo
  echo "# Review threads of PR $n"
  gh api graphql --paginate -F owner="$owner" -F name="$name" -F number="$n" -f query='
    query($owner: String!, $name: String!, $number: Int!, $endCursor: String) {
      repository(owner: $owner, name: $name) {
        pullRequest(number: $number) {
          reviewThreads(first: 100, after: $endCursor) {
            pageInfo { hasNextPage endCursor }
            nodes {
              id
              isResolved
              path
              line
              comments(first: 20) { nodes { databaseId author { login } body } }
            }
          }
        }
      }
    }' --jq '.data.repository.pullRequest.reviewThreads.nodes[]
      | "\n## \(.path):\(.line) resolved \(.isResolved) thread \(.id)\n"
        + (.comments.nodes | map("\n### \(.author.login) comment \(.databaseId)\n\n\(.body)") | join("\n"))'
} > "$out"

wc -l "$out"
```

Read `$out` one time. The file names each thread id and each comment id, so a reply needs no second query. The author replies with the commands of `gitar-review`. A reviewer writes no comment, and it records each thread under `## PR comments` in the review record (D-250).

## The staged read of a diff

A large diff costs more than the review needs in one read. Read it in three stages, and stop at the stage that answers the question.

```
git fetch origin
git diff --stat origin/main...HEAD
git diff --name-status origin/main...HEAD
git diff origin/main...HEAD -- <path>
```

The first line gives the size and the shape. The second line names each added, deleted, renamed, and modified path. The third line reads one path in full. Read every changed path before the verdict, and read the deleted and renamed paths too.
