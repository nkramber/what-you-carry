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

## The 53 rules in short form

### Section 1 - Words
- 1.1 Use only approved dictionary words, technical names, and technical verbs.
- 1.2 Use approved words only as the part of speech given.
- 1.3 Use approved words only with their approved meaning.
- 1.4 Use only approved forms of verbs and adjectives.
- 1.5 You can use words that fit a technical name category.
- 1.6 Use an unapproved word only when it is a technical name or part of one.
- 1.7 Do not use technical names as verbs.
- 1.8 Use technical names that agree with approved nomenclature.
- 1.9 Select technical names that are short and easy to understand.
- 1.10 Do not use slang or jargon as technical names.
- 1.11 Do not use different technical names for the same item.
- 1.12 You can use verbs that fit a technical verb category.
- 1.13 Do not use technical verbs as nouns.
- 1.14 Use American English spelling, unless an official directive says otherwise.

### Section 2 - Noun clusters
- 2.1 Write noun clusters of max three words.
- 2.2 Write a long technical name in full, then give a short name or use hyphens.
- 2.3 Use an article or demonstrative adjective before a noun.

### Section 3 - Verbs
- 3.1 Use only verb forms given in the dictionary.
- 3.2 Make only: infinitive, imperative, simple present, simple past, past participle as adjective, future.
- 3.3 Use the past participle only as an adjective.
- 3.4 Do not use helping verbs to make complex verb structures.
- 3.5 Use the "-ing" form only as a technical name or in a technical name.
- 3.6 Use the active voice in procedures. Use it as much as possible in descriptions.
- 3.7 Use an approved verb to describe an action, not a noun.

### Section 4 - Sentences
- 4.1 Write short and clear sentences.
- 4.2 Do not omit words or use contractions to make sentences shorter.
- 4.3 Use a vertical list for complex text.
- 4.4 Use connecting words to connect sentences with related topics.

### Section 5 - Procedures
- 5.1 Max 20 words in each sentence.
- 5.2 One instruction in each sentence, unless actions occur at the same time.
- 5.3 Write instructions in the imperative.
- 5.4 Divide a descriptive statement from the command with a comma.
- 5.5 Write notes only to give information, not instructions.

### Section 6 - Descriptions
- 6.1 Give information gradually.
- 6.2 Use key words and phrases to organize the text.
- 6.3 Max 25 words in each sentence.
- 6.4 Use paragraphs to show related information.
- 6.5 Each paragraph has only one topic.
- 6.6 No paragraph has more than six sentences.

### Section 7 - Safety instructions
- 7.1 Use a word such as "WARNING" or "CAUTION" to identify the risk level.
- 7.2 Start a safety instruction with a clear command or condition.
- 7.3 Give an explanation that shows the risk or the possible result.

### Section 8 - Punctuation and word count
- 8.1 Use all standard punctuation except the semicolon.
- 8.2 Use hyphens to connect closely related words.
- 8.3 Use parentheses for references, item identifiers, step identifiers, abbreviations, and singular/plural forms.
- 8.4 In a vertical list, a colon counts as the end of a sentence.
- 8.5 Text in parentheses counts as one word.
- 8.6 Count each number, unit, abbreviation, identifier, quoted text, and title as one word.
- 8.7 A hyphenated word counts as one word.

### Section 9 - Writing practices
- 9.1 Use a different construction when a word-for-word replacement is not enough.
- 9.2 Use each approved word correctly.
- 9.3 Do not make phrasal verbs.
- 9.4 Use a consistent style for terminology and wording.

## Technical names in this project

The rules permit these as written. They are technical names (rule 1.5):

- The game title: What You Carry.
- Tools and platforms: Godot, C#, .NET, xUnit, dotnet format, GitHub Actions, Steam, Steamworks, Steam Deck, Blockbench, JSON, JSONL, Metal, Vulkan, MoltenVK.
- The two harnesses: Claude Code, Codex.
- Project names: WhatYouCarry.Core, WhatYouCarry.Game, WhatYouCarry.Tools, WhatYouCarry.Tests, DetMath.
- Game terms: run, floor, stairwell, hunter, timer, boss, hub, bank, loadout, satchel, quick slot, amulet, skill orb, skill tree, skill point, affix, rarity, tier, band, potion, mana, cooldown, reload, hyper-armor, stagger, dodge, block, shield, intent, tick, seed, replay, voxel, greedy meshing, pathfinding, lighting, ambient occlusion.
- Process terms: session handoff, decision register, questions register, PR gate, cross-provider review, property test, bot sweep, seed sweep, smoke session, foundation gate.
- Save terms: profile file, run record, basic kit, simulation version, content hash.
- The standard itself: ASD-STE100, STE.
- Code identifiers in backticks.

Use one term per concept. Examples:

- "ascend", not "extract" or "cash out".
- "descend", not "go deeper".
- "bank", not "stash" or "vault".
- "satchel", not "backpack" or "bag".
- "stairwell", not "exit" or "stairs".
- "hunter", not "ghost" or "chaser".

## The checker

A C# tool, `WhatYouCarry.Tools.SteCheck`, will run on every hand-written `.md` file in the PR gate (D-130). The tool does not exist yet. Until it exists, apply the checklist by hand and state that you did in the PR.

The checker flags passive voice (3.6) and modal and helper verbs (3.2, 3.4). It flags sentence-initial and preposition-led -ing forms (3.5), and the 20-word limit in a numbered step (5.1). It also flags semicolons, contractions, and the 25-word limit. Dated records are exempt: review files, session handoffs, and audit files.

## Markdown notes

- Tables and code blocks are exempt from sentence-length counts. Keep cell text short.
- Headings are titles. They count as one word (8.6).
- The "plain-English" paragraphs in the design doc are descriptive text. Rule 6.3 applies (max 25 words).
