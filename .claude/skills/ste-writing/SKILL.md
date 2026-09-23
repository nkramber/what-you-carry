---
name: ste-writing
description: Write and review text in ASD-STE100 Simplified Technical English. Load before you write any .md, skill, or agent file in this repo.
---

# STE writing skill

Use this skill before you write text in this repo. The owner requires ASD-STE100 for every doc, skill, and agent file (D-139).

Source: ASD-STE100 Issue 8 (2021-04-30), Part 1, Writing rules. Issue 9 (2025-01) supersedes it with the same 53 rules. The full standard is free at https://www.asd-ste100.org/. This skill gives the 53 rules in short form. It does not copy the dictionary.

## Procedure

1. Write the text.
2. Check each sentence against the checklist below.
3. Correct each sentence that fails.
4. Read the text again as a reader who does not know the subject.

## Checklist (the rules that fail most often)

- Max 20 words in a procedural sentence. Max 25 words in a descriptive sentence (5.1, 6.3).
- One instruction per sentence (5.2).
- Instructions in the imperative: "Load the file." Not "The file should be loaded." (5.3).
- Active voice in procedures. Active voice as much as possible in descriptions (3.6).
- No "-ing" verb forms. "Sync the data" not "Syncing the data". The rules permit an "-ing" word only in a technical name (3.5).
- No helping verbs for complex tenses: "we did", not "we have been doing" (3.4).
- Tenses allowed: infinitive, imperative, simple present, simple past, past participle as adjective, future (3.2).
- No semicolons (8.1).
- No contractions (4.2).
- Max three words in a noun cluster. Write longer names in full, then use hyphens or a short name (2.1, 2.2).
- Use "the", "a", "this" before nouns (2.3).
- One term per concept. Do not use synonyms for variety (1.11, 9.4).
- Each paragraph: one topic, max six sentences (6.5, 6.6).
- Use vertical lists for complex content (4.3).
- Start a safety note with the risk word: WARNING, CAUTION (7.1).
- Notes give information, not instructions (5.5).
- American English spelling (1.14).
- No phrasal verbs: "remove" not "take out" (9.3).
- Do not use a technical name as a verb (1.7). Write "make a backup", not "backup the data".

## The 53 rules

The file `references/the-53-rules.md` gives every rule in short form, by section. Load it when a rule id needs its full text.

## Process terms

Use one term for one concept (rules 1.11 and 9.4). A session meets these terms in every document. Write the left term, and never a synonym.

| Term | What it names |
|---|---|
| session handoff | `docs/session-handoff.md`, the entry of each session, newest first (D-146) |
| decision register | `docs/decisions.md`, the owner decisions, D-1 onward |
| questions register | `docs/questions.md`, the open questions, OQ-1 onward |
| focused roadmap | one file under `docs/roadmaps/`, with the entry of each PR and its exit tests |
| review record | `docs/reviews/pr-<number>.md`, the verdict of the cross-provider review (D-101) |
| response file | `docs/reviews/pr-<number>-response.md`, the answer of the author to a review |
| PR gate | the list in the agent files that every PR passes before the merge |
| documents matrix | the `## Documents` section of the PR description, one line for each category (D-376) |
| cross-provider review | the review by the provider that did not write the PR (T-4) |
| automated pass | the review of gitar on a PR head, and the answer of the author to it (D-250) |
| effective head | the newest commit that changes a path outside the skip set. The cross-provider review reads it (D-534) |
| work head | the newest commit that changes a path outside the metadata set. The automated pass and the override label read it (D-184, D-539) |
| metadata set | `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md` (D-184) |
| skip set | the paths that count as documents for the CI skip of a PR head (D-475) |
| property test | a seed loop that asserts an invariant, and names its seed on a failure (D-66) |
| bot sweep | the bot policy runs of the night, at five thousand seeds |
| seed sweep | the reachability sweep of the night, at one hundred thousand seeds |
| smoke session | the headless Game session that the `smoke` job runs on three platforms (D-114) |
| night record | `night.json` on the branch `night-results`, which the `night-gate` job reads (D-273) |
| foundation gate | the gate that a phase passes before the next phase starts |
| exit test | one numbered test of a roadmap entry, which the gate of that entry names |

## Technical names of the project areas

The names of the game, the art, the save files, and the projects live in `references/technical-names.md`. Load it when you write about one of those areas.

## The byte ceilings

A session reads these files early, and each byte costs tokens on every later model call. A test fails when a file is over its ceiling. A larger ceiling needs a decision (D-382).

| File | Ceiling | Decision |
|---|---|---|
| `CLAUDE.md` and `AGENTS.md` | 15000 bytes each | D-382 |
| Each `.md` file under `.claude/skills/`, a reference file included | 12000 bytes | D-384 |
| `.claude/skills/one-pr-one-session/SKILL.md` | 7000 characters | D-375, D-385 |
| The newest entry of `docs/session-handoff.md` | 7000 bytes | D-382 |
| `docs/session-handoff.md` | 60000 bytes | D-382, D-379 |

The `handoff-rotate` command keeps the handoff under its ceiling (D-379). The archive has no ceiling, because no session reads it without a pointer.

When a file reaches its ceiling, move the detail to a reference file or to a register. Do not delete a rule to win bytes.

## The checker

The C# tool `WhatYouCarry.Tools.SteCheck` runs on every hand-written `.md` file in the PR gate (D-130). The `ste-check` CI job runs it on every PR. Run it before you commit:

```
dotnet run --project WhatYouCarry.Tools/WhatYouCarry.Tools.csproj -- ste-check --root .
```

The command prints one line per finding: the file, the line, the rule id, what the rule saw, and the sentence. It exits 1 on any finding. The rules and the exemptions:

| Rule id | What the checker flags |
|---|---|
| STE 5.1 | More than 20 words in a numbered item under a heading that holds "Sequence" or "Procedure" |
| STE 6.3 | More than 25 words in any other sentence |
| STE 8.1 | A semicolon |
| STE 4.2 | A contraction: `n't`, or a pronoun with `'s`, `'re`, `'ve`, `'ll`, `'d`, or `'m`. A possessive passes |
| STE 3.6 | Passive voice: is, are, was, were, be, been, or being, then a past participle. One adverb can stand between them |
| STE 3.4 | A helper verb: should, would, could, might, may, shall, ought. Also has, have, or had before a participle, and is or are before an -ing form |
| STE 3.5 | An -ing form as the first word of a sentence, or after a preposition |
| D-178 | A citation of a decision that the register marks `Superseded by D-N`, on a line with no revision word and without D-N |
| D-187 | Two `## Session <number>` headings with the same number in `docs/session-handoff.md` |

Dated records are exempt by path: `docs/reviews/`, `docs/session-handoff.md`, `docs/session-handoff-archive.md`, and `docs/archive/`. The reference check also skips them, because a dated record is history, and a rewrite to name the reviser falsifies it.

The passive and participle rules are heuristics. A past participle is an irregular form from a list, or a word that ends in "ed". "is closed" is a finding, and so is "is required". Rewrite the sentence with the actor as the subject: "the build needs the SDK". "must", "can", and "will" pass, because the standard approves them.

An -ing word that is a noun or a technical name passes: nothing, during, warning, heading, finding, lighting, meshing, pathfinding, and a few more. A hyphenated word never counts as an -ing form. To add a technical name, add it to the list in `SteRules.cs` with a test.

## Markdown notes

- Tables, fenced code blocks, and thematic breaks are exempt from every rule. Keep cell text short.
- Headings are titles. They count as one word (8.6). The checker reads no rule on a heading.
- Text in backticks, in double quotes, or in parentheses is one word (8.5, 8.6). The grammar rules do not read inside it.
- A colon that a space or the line end follows ends a sentence, in any text and not only in a vertical list (8.4). A colon inside a word, as in a time or a URL, does not.
- Parentheses can nest. The complete outer span is one word.
- A sentence stays on one line. The checker reads each line alone, so a sentence that wraps to a second line counts as two.
- A numbered item is a procedural step only under a heading that holds "Sequence" or "Procedure". The nearest heading above the item decides, at any level.
- The "plain-English" paragraphs in the design doc are descriptive text. Rule 6.3 applies (max 25 words).
