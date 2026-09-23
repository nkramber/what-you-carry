---
name: gitar-review
description: Get a Gitar review of the head of a pull request. Wait three minutes after each push, prove that the review is current, and answer every finding. Verify each finding as a claim, then fix and reply, or refute, reply, and resolve. Load after each push to a pull request, documents alone included.
---

# Gitar review skill

The GitHub app `gitar-bot` reviews pull requests. This skill gets a Gitar review of the head of a pull request, and then answers each finding. A pull request of documents alone waits for the review too.

Each repo that uses Gitar keeps a copy of this file. A rule of the repo wins over this skill. For example, a repo can ask for a second review, or it can limit who replies to Gitar.

## Terms

- **Head**: the newest commit of the pull request branch on GitHub.
- **Metadata set**: the paths that hold a record of the work and no work: `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md` (D-184).
- **Metadata commit**: a commit that changes paths inside the metadata set alone.
- **Effective head**: the newest commit that changes a path outside the metadata set (D-184). A metadata commit does not move it.
- **Dashboard comment**: the Gitar comment on the pull request that holds the collapsed `Code Review` block. Gitar edits this comment for each review. Gitar can also delete it and post a new one with a new id.
- **Pause note**: the note at the top of the dashboard comment that starts "Automatic reviews are paused".
- **Manual review**: the review that a `Gitar review` comment starts.
- **Current review**: a review of the head.
- **Stale review**: a review of a commit older than the head.
- **Push wait**: the minimum wait of three minutes after a push, before a `Gitar review` comment (D-160).

## Why a review goes stale

Gitar reviews each push until it pauses automatic reviews. After the pause, a push starts no review, and the dashboard comment keeps the review of an older commit. The dashboard comment names no commit. Thus a stale review looks the same as a current review.

A manual review also goes stale when a push comes after the `Gitar review` comment. On 2026-09-16, four pull requests in two repos had a review older than the head. Each one had a push after the last request, or a push and no request.

## The effective head

This repository reviews the effective head, and not the tip (D-184). A commit of metadata records the work, and it changes no code, no content, no configuration, and no test. Such a commit keeps a pass current.

- A push that changes a path outside the metadata set moves the effective head. Start the procedure below for that new head.
- A push of metadata alone keeps the pass of the earlier head current. Do not ask for a review again.
- The commit that holds the review record and the handoff entry is always a metadata commit (D-182). Without this rule that commit invalidates the pass that it publishes.
- Read the effective head with the staged read of `docs/runbooks/session-context.md`. Take the newest commit that names a path outside the metadata set.

```
git log --format='%H %s' origin/main..HEAD --name-only
```

Every condition below reads the effective head where it says head. The tip of the branch can be newer.

## Procedure

Do these steps after each push.

1. Push all the commits of this round of changes. Push one time, not one time for each fix.
2. Run command A. Continue only when the local head and the pull request head are the same commit.
3. Record the effective head and the push time from command A. A push of metadata alone ends here, because the pass stays current.
4. Do the push wait with command E. Always do the full push wait, also when Gitar paused automatic reviews.
5. Do not comment `Gitar review` before the push wait ends.
6. After the push wait, run the Gitar check part of command E. Then run command B.
7. Find if an automatic review started. Apply the rule in "Find an automatic review".
8. When an automatic review runs, wait until its Gitar check completes. Do not comment `Gitar review`.
9. Apply the rule in "Prove that a review is current".
10. When the review is current, go to step 17.
11. When the review is stale, or you cannot prove that it is current, comment `Gitar review` on the pull request.
12. Read the Gitar reply to that comment with command B. When the reply is "On it", go to step 15.
13. When the reply is "You've sent several Gitar comments in a short window", no review started. Wait ten minutes, then go to step 11.
14. When no reply comes in five minutes, go to step 11.
15. Do not push while the manual review runs. A push at this time makes the review stale.
16. When Gitar edits or replaces the dashboard comment, go to step 9.
17. Open the collapsed `Code Review` block of the dashboard comment. Read the summary.
18. Export every comment and thread with command F. Read the file one time, and read each open thread.
19. Read each finding as a claim, not a fact. Reproduce its trigger. Read the rule or the decision it names.
20. Decide the merit of the finding: full, partial, or none.
21. For full merit, make the smallest change that fixes the finding. Commit it. Before the commit, run the checks of D-491 for documents alone, or the full suite for code (D-493).
22. For no merit, reply on the thread with the reason and the evidence. Then resolve the thread.
23. For partial merit, fix the part with merit. Refute the rest in the same reply.
24. When you have commits, go to step 1. After the push, reply on each thread with the commit that fixes it.
25. Stop when a current review approves, or when a current review adds no finding and each finding has its answer.
26. Tell the owner that the pull request is ready to merge.

## Find an automatic review

The owner permits a `Gitar review` comment only after the push wait, and only when no automatic review started (D-160).

An automatic review started when one of these conditions is true:

- The Gitar check of `gitar-bot` on the head has the status `queued` or `in_progress`.
- The review is current. Apply the rule in "Prove that a review is current".

When neither condition is true after the push wait, no automatic review started. You can then comment `Gitar review`.

On 2026-09-16, the Gitar check on the heads of #30, #31, and #32 started 8 to 31 seconds after the commit. Each check completed in 80 seconds or less. So the push wait of three minutes is longer than a normal automatic review.

## Prove that a review is current

A review is current only when each of these conditions is true:

- The effective head from command B is the head that you recorded in step 3.
- The dashboard comment has an edit time later than the push time that you recorded in step 3.
- After a `Gitar review` comment, Gitar replied "On it", and the dashboard comment has an edit time later than that reply.
- You read the newest dashboard comment. Gitar can delete the dashboard comment and post a new one with a new id.

The summary is not a condition. A review that adds no finding can keep the summary of the older review, word for word. On 2026-09-16, the review of a correction push did this, and its three times proved it current. Do not ask for a review again only because the summary did not change.

When one condition is false, the review is stale. When you cannot check one condition, treat the review as stale. A request for a manual review costs little. A merge on a stale review costs more.

## Rules for each reply

- State the evidence: the command, the test, the decision id, or the commit.
- Follow the attribution rule of the repo.
- Never accept a finding only to close the review faster. A wrong fix costs more than a written disagreement.
- Never make a fix larger than the rule that the finding names.
- When a finding conflicts with an owner decision, quote both and ask the owner.

## Traps

The file `references/traps.md` lists the cases that cost a session a wrong answer. Read it before the first review of a pull request, and again when a wait does not end.

## Commands

Set the three variables once in each shell. The commands read the repo from the working directory.

```bash
repo=$(gh repo view --json nameWithOwner --jq .nameWithOwner)
owner=${repo%/*}
name=${repo#*/}
n=<number>
```

### A. The head check

```bash
# The two commit ids must be the same. Record the head and the push time.
git rev-parse HEAD
gh pr view "$n" --json headRefOid --jq .headRefOid
date -u +%Y-%m-%dT%H:%M:%SZ
# The push time in seconds, for the push wait of command E.
date +%s
```

### B. The freshness check

```bash
# The head now. Compare it with the head of command A.
echo "head:      $(gh pr view "$n" --json headRefOid --jq .headRefOid)"

# The time of the newest "Gitar review" comment, if any.
echo "requested: $(gh api --paginate "repos/$repo/issues/$n/comments" \
  --jq '.[] | select(.body | test("^\\s*gitar review\\s*$"; "i")) | .created_at' | tail -1)"

# The time and the first line of the newest Gitar reply to a request: "On it", or a refusal.
echo "reply:     $(gh api --paginate "repos/$repo/issues/$n/comments" \
  --jq '.[] | select(.user.login == "gitar-bot[bot]")
        | select(.body | test("^> gitar review"; "i"))
        | "\(.created_at) \(.body | split("\n")[2])"' | tail -1)"

# The id and the last edit time of the newest dashboard comment.
echo "dashboard: $(gh api --paginate "repos/$repo/issues/$n/comments" \
  --jq '.[] | select(.user.login == "gitar-bot[bot]")
        | select(.body | test("<b>Code Review</b>"))
        | "\(.id) \(.updated_at)"' | tail -1)"

# The dashboard text. Replace <id> with the id above.
gh api "repos/$repo/issues/comments/<id>" --jq .body
```

### C. The review threads

```bash
gh api graphql -F owner="$owner" -F name="$name" -F number="$n" -f query='
  query($owner: String!, $name: String!, $number: Int!) {
    repository(owner: $owner, name: $name) {
      pullRequest(number: $number) {
        reviewThreads(first: 100) {
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
  }'
```

### D. Replies and requests

```bash
# The checks of the pull request, the Gitar check included
gh pr checks "$n"

# Reply on a thread. The comment id is the databaseId of the first comment.
gh api "repos/$repo/pulls/$n/comments/<comment-id>/replies" -f body='<reply>'

# Resolve a thread. The thread id comes from command C.
gh api graphql -f id=<thread-id> -f query='
  mutation($id: ID!) {
    resolveReviewThread(input: {threadId: $id}) { thread { isResolved } }
  }'

# Ask for a manual review
gh pr comment "$n" --body "Gitar review"
```

### E. The push wait and the Gitar check

The runbook `docs/runbooks/session-context.md` holds both commands under "Wait for gitar". Do the full push wait of three minutes, and then read the Gitar check of the head.

### F. The comment export

One export reads every issue comment and every review thread into one file. The runbook holds the command under "The comment export". Read the file one time, and reply with the commands of section D.
