using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WhatYouCarry.Tools;
using WhatYouCarry.Tools.DocGate;
using WhatYouCarry.Tools.ReviewGate;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The documentation gate (D-375, D-376): the rules over fixture descriptions and diffs, the read of real commits,
/// and the exit codes of the command.
/// </summary>
[Collection(ConsoleCollection.Name)]
public sealed class DocGateTests
{
    private const string Branch = "feat/pr-70-sample";
    private const string Title = "feat: add the sample change";

    private static readonly string[] CodeAndDocs = ["WhatYouCarry.Core/Sample.cs", "WhatYouCarry.Tests/SampleTests.cs", "docs/design.md", "docs/decisions.md", "docs/roadmaps/phase-2-first-playable.md", "docs/session-handoff.md"];

    [Fact]
    public void PullRequestWithCodeAndEveryAffectedDocumentPasses()
    {
        DocGateResult result = DocGateRules.Evaluate(Facts(Body(), CodeAndDocs));

        Assert.True(result.Passes, string.Join("\n", result.Problems));
    }

    [Fact]
    public void SpecificNoChangeReasonPasses()
    {
        // A PR that changes no document of a category, and gives the specific reason, passes.
        string body = Body(("`docs/design.md`", "Reviewed; no change needed: section 3.14 already names the rule that this test fix keeps."));
        string[] paths = CodeAndDocs.Where(path => path != "docs/design.md").ToArray();

        DocGateResult result = DocGateRules.Evaluate(Facts(body, paths));

        Assert.True(result.Passes, string.Join("\n", result.Problems));
    }

    [Fact]
    public void MissingHandoffFails()
    {
        string[] paths = CodeAndDocs.Where(path => path != DocGateRules.HandoffPath).ToArray();
        string body = Body(("`docs/session-handoff.md`", "Reviewed; no change needed: the entry of this session sits in another place."));

        DocGateResult result = DocGateRules.Evaluate(Facts(body, paths, handoffEntry: null));

        Assert.False(result.Passes);
        Assert.Contains(result.Problems, problem => problem.Contains($"does not change {DocGateRules.HandoffPath}", StringComparison.Ordinal));
    }

    [Fact]
    public void HandoffEntryOfAnotherBranchFails()
    {
        DocGateResult result = DocGateRules.Evaluate(Facts(Body(), CodeAndDocs, Entry("docs/pr-68-merge-notes")));

        Assert.False(result.Passes);
        Assert.Contains(result.Problems, problem => problem.Contains($"Branch `{Branch}`", StringComparison.Ordinal));
    }

    [Fact]
    public void DeferredDocumentsInTheDescriptionFail()
    {
        string[] deferrals =
        [
            "The owner merges, and a docs PR records the merge.",
            "A follow-up documentation PR adds the roadmap line.",
            "We will update the design doc next week.",
            "Record the status in the roadmap after the merge.",
            "Roadmap status: TBD.",
            "The roadmap mark follows in a docs PR after the merge.",
        ];
        foreach (string deferral in deferrals)
        {
            DocGateResult result = DocGateRules.Evaluate(Facts(Body() + "\n## Next\n\n" + deferral + "\n", CodeAndDocs));

            Assert.False(result.Passes, deferral);
            Assert.Contains(result.Problems, problem => problem.StartsWith("The PR description defers documents", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void RuntimeBehaviorTextIsNotADeferral()
    {
        // A sentence about what a job does is not a promise of documents in later work.
        string body = Body() + "\n## Behavior\n\nThe night job will update the record on main after each run.\n";

        DocGateResult result = DocGateRules.Evaluate(Facts(body, CodeAndDocs));

        Assert.True(result.Passes, string.Join("\n", result.Problems));
    }

    [Fact]
    public void DeferredDocumentsInTheNewestHandoffEntryFail()
    {
        // The next action of Session 174 took this form under D-297, which D-375 supersedes.
        string entry = Entry(Branch) + "The owner then merges with the red `night-gate` (D-372), and a docs PR records the merge (D-297).\n";

        DocGateResult result = DocGateRules.Evaluate(Facts(Body(), CodeAndDocs, entry));

        Assert.False(result.Passes);
        Assert.Contains(result.Problems, problem => problem.StartsWith("The newest handoff entry defers documents", StringComparison.Ordinal));
    }

    [Fact]
    public void MergeRecordTitleOrBranchFails()
    {
        DocGateResult byTitle = DocGateRules.Evaluate(Facts(Body(), CodeAndDocs, title: "docs: record the merge of PR-68 as PR #75"));
        DocGateResult byOldTitle = DocGateRules.Evaluate(Facts(Body(), CodeAndDocs, title: "docs: record the PR-11 merge and file the PR-58 questions"));
        DocGateResult byBranch = DocGateRules.Evaluate(Facts(Body(), CodeAndDocs, Entry("docs/pr-68-merge-record"), branch: "docs/pr-68-merge-record"));

        Assert.False(byTitle.Passes);
        Assert.False(byOldTitle.Passes);
        Assert.False(byBranch.Passes);
        Assert.All([byTitle, byOldTitle, byBranch], result => Assert.Contains(result.Problems, problem => problem.Contains("names a merge record", StringComparison.Ordinal)));
    }

    [Fact]
    public void MissingOrIncompleteMatrixLineFails()
    {
        string missing = Body().Replace("- `docs/runbooks/`: Not applicable: the change touches no runbook and no machine setup step.\n", string.Empty, StringComparison.Ordinal);
        string empty = Body(("`docs/questions.md`", string.Empty));
        string generic = Body(("`docs/questions.md`", "Reviewed; no change needed: no documentation impact."));
        string shortReason = Body(("`docs/questions.md`", "Not applicable: nothing."));

        Assert.Contains(DocGateRules.Evaluate(Facts(missing, CodeAndDocs)).Problems, problem => problem.Contains("no line for `docs/runbooks/`", StringComparison.Ordinal));
        Assert.Contains(DocGateRules.Evaluate(Facts(empty, CodeAndDocs)).Problems, problem => problem.Contains("does not start with", StringComparison.Ordinal));
        Assert.Contains(DocGateRules.Evaluate(Facts(generic, CodeAndDocs)).Problems, problem => problem.Contains("generic reason", StringComparison.Ordinal));
        Assert.Contains(DocGateRules.Evaluate(Facts(shortReason, CodeAndDocs)).Problems, problem => problem.Contains("generic reason", StringComparison.Ordinal));
    }

    [Fact]
    public void DuplicateMatrixLineFails()
    {
        // PR #77 review P2-1: a second line for a category, after a valid first line, made the disposition ambiguous and passed.
        string body = Body() + "- `docs/design.md`: Not applicable: no design changes affect this pull request.\n";

        DocGateResult result = DocGateRules.Evaluate(Facts(body, CodeAndDocs));

        Assert.False(result.Passes);
        Assert.Contains(result.Problems, problem => problem.Contains("2 lines for `docs/design.md`", StringComparison.Ordinal));
    }

    [Fact]
    public void MatrixLineThatDisagreesWithTheDiffFails()
    {
        string claimsNoChange = Body(("`docs/design.md`", "Reviewed; no change needed: section 3.14 already names the rule."));
        string claimsChange = Body(("`docs/questions.md`", "Changed: OQ-177 records the question of the sample change."));

        Assert.Contains(DocGateRules.Evaluate(Facts(claimsNoChange, CodeAndDocs)).Problems, problem => problem.Contains("the diff changes a path of that category", StringComparison.Ordinal));
        Assert.Contains(DocGateRules.Evaluate(Facts(claimsChange, CodeAndDocs)).Problems, problem => problem.Contains("the diff changes no path of that category", StringComparison.Ordinal));
    }

    [Fact]
    public void CommentedMatrixLineDoesNotCount()
    {
        string body = Body().Replace("- `docs/questions.md`:", "<!-- - `docs/questions.md`:", StringComparison.Ordinal)
            .Replace("sample change.\n- `docs/roadmaps/`", "sample change. -->\n- `docs/roadmaps/`", StringComparison.Ordinal);

        DocGateResult result = DocGateRules.Evaluate(Facts(body, CodeAndDocs));

        Assert.Contains(result.Problems, problem => problem.Contains("no line for `docs/questions.md`", StringComparison.Ordinal));
    }

    [Fact]
    public void HandoffCommitStaysInTheMetadataSet()
    {
        // D-184: the required handoff commit never moves the effective head, so a review record stays valid for it.
        Assert.Contains(DocGateRules.HandoffPath, ReviewGateRules.MetadataPaths);
        Assert.Contains("docs/session-handoff-archive.md", ReviewGateRules.MetadataPaths);
    }

    [Fact]
    public void GatherReadsTheDiffAndTheCommittedHandoff()
    {
        using var repo = new TemporaryGitRepository();
        repo.Commit("chore: base", Files(("README.md", "base"), (DocGateRules.HandoffPath, "# Session handoff\n")));
        repo.CreateBranch(Branch);
        string head = repo.Commit("feat: sample", Files(("WhatYouCarry.Core/Sample.cs", "// sample"), (DocGateRules.HandoffPath, "# Session handoff\n\n" + Entry(Branch))));
        File.WriteAllText(Path.Combine(repo.Path, DocGateRules.HandoffPath), "# Session handoff\n\n" + Entry("feat/uncommitted"));
        string bodyPath = Path.Combine(repo.Path, "body.md");
        File.WriteAllText(bodyPath, "body");

        DocGateFacts facts = DocGateFacts.Gather(repo.Path, "main", head, bodyPath, Title, Branch);

        Assert.Equal(["WhatYouCarry.Core/Sample.cs", DocGateRules.HandoffPath], facts.ChangedPaths.OrderBy(path => path, StringComparer.Ordinal));
        Assert.NotNull(facts.NewestHandoffEntry);
        Assert.Contains($"Branch `{Branch}`", facts.NewestHandoffEntry, StringComparison.Ordinal);
        Assert.Equal("body", facts.Body);
        Assert.Equal([new CommitMessage(head, "feat: sample\n")], facts.CommitMessages);
    }

    [Fact]
    public void EachAttributionFormFailsAndNamesWhereItWasFound()
    {
        // F-138: T-6 had no machine check. Each form of G-13 fails in each place that the gate reads, and the problem
        // names the place, the line, and the form. The case of a trailer does not matter.
        string robot = char.ConvertFromUtf32(0x1F916);
        (string Title, string BodyPrefix, string Commit, string Expected)[] cases =
        [
            (Title, string.Empty, "feat: sample\n\nThe change.\n\nCo-authored-by: A Person <a@example.invalid>\n", "The message of commit 1a2b3c4, line 5, holds a co-author trailer"),
            (Title, string.Empty, "feat: sample\n\nco-authored-by: a person\n", "The message of commit 1a2b3c4, line 3, holds a co-author trailer"),
            (Title, "CO-AUTHORED-BY: A Person\n", "feat: sample\n", "The PR description, line 1, holds a co-author trailer"),
            (Title, "<!--\nCo-authored-by: A Person\n-->\n", "feat: sample\n", "The PR description, line 2, holds a co-author trailer"),
            (Title, $"{robot} Generated with [Claude Code](https://example.invalid/claude-code)\n", "feat: sample\n", "The PR description, line 1, holds a robot line"),
            (Title, $"{robot} Generated with [Claude Code](https://example.invalid/claude-code)\n", "feat: sample\n", "The PR description, line 1, holds a generation line"),
            (Title, $"  {robot} A tool wrote this change.\n", "feat: sample\n", "The PR description, line 1, holds a robot line"),
            ("feat: add the sample change, generated by Codex", string.Empty, "feat: sample\n", "The PR title, line 1, holds a generation line"),
            (Title, string.Empty, "feat: sample\n\nGenerated with ChatGPT.\n", "The message of commit 1a2b3c4, line 3, holds a generation line"),
            (Title, string.Empty, "feat: sample\n\nThe text was generated by `Copilot`.\n", "The message of commit 1a2b3c4, line 3, holds a generation line"),
            (Title, string.Empty, "feat: sample\n\nGenerated with GPT-4o.\n", "The message of commit 1a2b3c4, line 3, holds a generation line"),
        ];
        foreach ((string title, string bodyPrefix, string commit, string expected) in cases)
        {
            DocGateResult result = DocGateRules.Evaluate(Facts(bodyPrefix + Body(), CodeAndDocs, title: title, commits: [new CommitMessage("1a2b3c4", commit)]));

            Assert.False(result.Passes, expected);
            Assert.Contains(result.Problems, problem => problem.StartsWith(expected, StringComparison.Ordinal) && problem.Contains("(T-6, D-137, D-176, G-13)", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void ABareProviderNamePasses()
    {
        // F-138, D-176: the rule is no wider than T-6. The review command, the review record, the merge summary, and
        // the handoff author field name the providers, and a line that names Codex as the reviewer is not attribution.
        // A trailer in running text is not a trailer, and a repository tool that generates a file is not an agent.
        const string lines = """
            - Cross-provider review by Codex per the `pr-review` skill (T-4).
            The author runs `make codex-review PR=104`, and the review record names Codex as the reviewer.
            Author: Claude Code
            The merge summary names What, How, CI, and the Codex review (D-533).
            The gate fails on a `Co-authored-by:` trailer and on a generation line.
            The atlas is generated by `tools texture-gen`, and the prompt is generated by codex-review.
            """;
        DocGateResult result = DocGateRules.Evaluate(Facts(Body() + "\n## Review\n\n" + lines, CodeAndDocs, commits: [new CommitMessage("1a2b3c4", "docs: the review record\n\n" + lines)]));

        Assert.True(result.Passes, string.Join("\n", result.Problems));
    }

    [Fact]
    public void TheTextOfPullRequest104Passes()
    {
        // F-138: the title of PR #104, the lines of its description that name a provider or the attribution rule, and
        // each of its commit messages, as git log and the PR view gave them on 2026-09-25. The rule must pass them.
        const string title = "fix: the repository review fixes of PR-88: a death at the stairwell, loud engine failures, and gates that read what they judge (PR-88)";
        const string bodyLines = """
            Each line holds before the merge, by auto-merge or by the owner (`CLAUDE.md`, PR gate, D-516).
            - [ ] The owner confirmed the merge after the merge summary: What, How, CI, and Codex review (D-524, D-533).
            - [x] No attribution anywhere (T-6). No commit subject or body names an agent, harness, or model as the source of the work (D-176).
            - `CLAUDE.md` and `AGENTS.md`: Reviewed; no change needed: the PR gate and the tenets already state each rule that these fixes restore.
            - `.claude/skills/`: Changed: the review record reference of `pr-review` states that the gate reads the bold verdict name on the first line.
            """;
        DocGateResult result = DocGateRules.Evaluate(Facts(Body() + "\n## Gate\n\n" + bodyLines, CodeAndDocs, title: title, commits: PullRequest104Commits));

        Assert.Equal(20, PullRequest104Commits.Length);
        Assert.True(result.Passes, string.Join("\n", result.Problems));
    }

    [Fact]
    public void CommandFailsOnACoAuthorTrailerInACommitOfTheRange()
    {
        // F-138: the command reads each commit message from the base to the head, and not the base itself.
        using var repo = new TemporaryGitRepository();
        repo.Commit("chore: base\n\nCo-authored-by: Base Person <base@example.invalid>", Files(("README.md", "base")));
        repo.CreateBranch(Branch);
        string clean = repo.Commit("feat: sample", Files(("WhatYouCarry.Core/Sample.cs", "// sample"), ("docs/design.md", "design"), ("docs/decisions.md", "decisions"), ("docs/roadmaps/phase-2-first-playable.md", "roadmap"), (DocGateRules.HandoffPath, "# Session handoff\n\n" + Entry(Branch))));
        string trailer = repo.Commit("fix: sample\n\nThe fix.\n\nCo-authored-by: A Person <a@example.invalid>", Files(("WhatYouCarry.Core/Sample.cs", "// fixed")));
        string bodyPath = Path.Combine(repo.Path, "body.md");
        File.WriteAllText(bodyPath, Body());

        DocGateFacts facts = DocGateFacts.Gather(repo.Path, "main", "HEAD", bodyPath, Title, Branch);
        Assert.Equal([new CommitMessage(trailer, "fix: sample\n\nThe fix.\n\nCo-authored-by: A Person <a@example.invalid>\n"), new CommitMessage(clean, "feat: sample\n")], facts.CommitMessages);

        TextWriter original = Console.Out;
        var output = new StringWriter();
        Console.SetOut(output);
        int exitCode;
        try
        {
            exitCode = Program.Main(["doc-gate", "--root", repo.Path, "--base", "main", "--head", "HEAD", "--body", bodyPath, "--title", Title, "--branch", Branch]);
        }
        finally
        {
            Console.SetOut(original);
        }

        Assert.Equal(1, exitCode);
        Assert.Contains($"doc-gate: The message of commit {trailer}, line 5, holds a co-author trailer: 'Co-authored-by: A Person <a@example.invalid>'.", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("doc-gate: fail, 1 problem(s) over 5 changed path(s) and 2 commit(s).", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void CommandExitCodesNameTheOutcome()
    {
        using var repo = new TemporaryGitRepository();
        repo.Commit("chore: base", Files(("README.md", "base")));
        repo.CreateBranch(Branch);
        repo.Commit("feat: sample", Files(("WhatYouCarry.Core/Sample.cs", "// sample"), ("docs/design.md", "design"), ("docs/decisions.md", "decisions"), ("docs/roadmaps/phase-2-first-playable.md", "roadmap"), (DocGateRules.HandoffPath, "# Session handoff\n\n" + Entry(Branch))));
        string passing = Path.Combine(repo.Path, "passing.md");
        string failing = Path.Combine(repo.Path, "failing.md");
        File.WriteAllText(passing, Body());
        File.WriteAllText(failing, Body() + "\nA docs PR records the merge.\n");

        TextWriter original = Console.Out;
        var output = new StringWriter();
        Console.SetOut(output);
        int pass;
        int fail;
        int usage;
        try
        {
            pass = Program.Main(["doc-gate", "--root", repo.Path, "--base", "main", "--head", "HEAD", "--body", passing, "--title", Title, "--branch", Branch]);
            fail = Program.Main(["doc-gate", "--root", repo.Path, "--base", "main", "--head", "HEAD", "--body", failing, "--title", Title, "--branch", Branch]);
            usage = Program.Main(["doc-gate", "--root", repo.Path]);
        }
        finally
        {
            Console.SetOut(original);
        }

        Assert.Equal(0, pass);
        Assert.Equal(1, fail);
        Assert.Equal(2, usage);
        Assert.Contains("doc-gate: fail, 1 problem(s)", output.ToString(), StringComparison.Ordinal);
    }

    /// <summary>The 20 commit messages of PR #104 from its base to d4b8d01, newest first, as the log gave them on 2026-09-25.</summary>
    private static readonly CommitMessage[] PullRequest104Commits =
    [
        new("d4b8d01", """
            docs: F-129 fixed, F-131, and the five concerns in the PR-88 records
            """),
        new("7387cab", """
            fix: an unknown band, a rotated locator, and an empty string fail at load, a keyframe tick gives the keyframe, and the ramp march test bounds its hit (PR-88)

            The floor band is checked against the three bands of D-210 at load. A locator
            with a rotation is an error, as a box or a bone with one is. An empty string
            value is an error. The pose at a keyframe tick is the keyframe, so a rotation
            near the float limit never overflows (F-131). The ramp march test asserts the
            hit and both bounds of its distance (F-129).
            """),
        new("40b6919", """
            docs: the gitar fence finding in the session 250 handoff
            """),
        new("0d219b1", """
            fix: a fence of a review record opens after no more than three spaces, as in Markdown (PR-88)

            A line with four spaces or a tab first is an indented code line, so the gate
            reads the sections after it as a reader sees them (F-116).
            """),
        new("4c55f16", """
            docs: session 250 handoff for the PR-104 corrections
            """),
        new("c5934c3", """
            docs: PR-104 review response, round 1
            """),
        new("f280b72", """
            fix: a finding status reads its complete form, and only a matching fence closes a fenced block (PR-88)

            A closed status names its revision or its decision, so a status such as
            'fixed.' is a fault and never closes a finding (F-125, PR #104 P1-1). The
            review record parse closes a fence only on a run of the same character that
            is at least as long, so a tilde line inside a backtick fence stays inside
            (F-116, PR #104 P1-2).
            """),
        new("1536fe9", """
            docs: verify PR-104 review publication
            """),
        new("da8c2c6", """
            docs: record PR-104 review
            """),
        new("35c389c", """
            docs: renumber the tail of the Phase 2 sequence and the PR-88 exit tests
            """),
        new("756d539", """
            docs: mark PR-88 done in PR #104
            """),
        new("7207164", """
            docs: F-120 to F-130, the PR-88 scope and exit tests, and the session 248 handoff
            """),
        new("4029e36", """
            fix: errors of a run name the seed and the floor, the session end cannot hang, content bounds fit their consumers, and tests check what they claim (PR-88)

            An error inside a tick or a replay carries the seed, the floor, and the tick,
            and a runtime error becomes its inner error. A failed descent leaves the old
            floor whole. The failure line names the error type and the inner chain, and
            the frame log failure names the tick of the end (F-121).

            An empty path word stops the boot, the quit always reaches the engine, a
            boot failure writes no frame log, and git ignores the relative outputs of the
            Game commands (F-122).

            Each numeric content field fits the type that holds it, and a projectile
            damage is one or more (F-120). The determinism test compares each tick, the
            worker test compares the enemy spawns, the ray test bounds the hit, the F-111
            seeds must reach the bottom with no enemy, and three reachability properties
            have checks (F-124).
            """),
        new("4d013f4", """
            fix: the gate tools match exact files, read known finding states, refuse a future night, fetch the branch ref, and build before a promotion (PR-88)

            A directory with the name of a root document no longer hides a code change
            from the effective head. An unknown finding status is a fault. A night record
            that ends in the future fails the gate and the promotion. The night fetch
            reads refs/heads, so a tag of the same name never takes the place of the
            branch. night-promote builds the tools first, so a build failure fails the
            job. git reads its two streams at once, and codex-review with the skip flag
            reads no gitar facts (F-125).
            """),
        new("62acff9", """
            fix: the state hash reads the path and the wedge count of each follower, and the Core tables are read-only (PR-88)

            PathFollower.AddTo folds the stored path and the wedge count (D-160, F-123).
            The bit-identity known answer moves to f1c35ddccb2cd0bb, and the simulation
            version stays 17 in this PR. The timer and move tables of Core are read-only
            lists, so no caller can change them for every run in the process (F-127).
            """),
        new("1f94f16", """
            docs: D-581, no interim change of the fork approval before PR-86
            """),
        new("43e55a9", """
            docs: the ruleset and comment export runbooks write each temporary file with mktemp (PR-88)
            """),
        new("a2a8e34", """
            docs: session 248 handoff for PR-88
            """),
        new("7074a35", """
            docs: the PR-88 decisions, questions, findings, and roadmap entry

            D-578 to D-580, OQ-195 to OQ-205, F-113 to F-119, and the PR-88 entry of the
            Phase 2 roadmap. The review record reference states the verdict line rule.
            """),
        new("701cb64", """
            fix: a death at the stairwell stays a death, the engine callbacks fail loud, and the gates read what they judge (PR-88)

            A lethal hit on the tick of a stairwell press ends the run as a death, and a
            descend on the deepest floor does nothing (D-322, D-579, F-113, F-114). The
            simulation version rises to 17, and the bit-identity known answer moves.

            Each engine callback of Main.cs catches every exception, writes one error
            line, and quits with exit code 1 (F-115). The review gate reads the verdict
            from the first line of its section, outside each fence (F-116). The seed
            sweep names a seed whose dig throws (F-117). The asset parse rejects a
            repeated key, and asset-qa reads each depth of the model directory (F-118,
            F-119). This PR holds more than one concern (D-580).
            """),
    ];

    private static DocGateFacts Facts(string body, IReadOnlyList<string> paths, string? handoffEntry = "", string title = Title, string branch = Branch, IReadOnlyList<CommitMessage>? commits = null)
    {
        return new DocGateFacts
        {
            Title = title,
            Branch = branch,
            Body = body,
            ChangedPaths = paths,
            NewestHandoffEntry = handoffEntry == string.Empty ? Entry(branch) : handoffEntry,
            CommitMessages = commits ?? [],
        };
    }

    private static string Entry(string branch)
    {
        return $"## Session 178: 2026-09-16, Claude Code\n\nAuthor: Claude Code\nSession: add the sample change. Branch `{branch}`.\n\n### In flight\n\nThe owner merges this PR.\n";
    }

    /// <summary>A description with a complete matrix for <see cref="CodeAndDocs"/>, with each given line replaced.</summary>
    private static string Body(params (string Label, string Text)[] replacements)
    {
        var lines = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["`docs/design.md`"] = "Changed: section 3.14 names the sample rule of this change.",
            ["`docs/decisions.md`"] = "Changed: D-377 records the owner answer on the sample.",
            ["`docs/questions.md`"] = "Reviewed; no change needed: no open question binds the sample change.",
            ["`docs/roadmaps/`"] = "Changed: the Phase 2 roadmap marks PR-70 done in this PR.",
            ["`docs/runbooks/`"] = "Not applicable: the change touches no runbook and no machine setup step.",
            ["`docs/session-handoff.md`"] = "Changed: Session 178 describes this PR and names its branch.",
            ["`CLAUDE.md` and `AGENTS.md`"] = "Reviewed; no change needed: no command, gate, or rule of the agent files changes.",
            ["`.claude/skills/`"] = "Reviewed; no change needed: no skill procedure reads the sample change.",
        };
        foreach ((string label, string text) in replacements)
        {
            lines[label] = text;
        }

        string matrix = string.Concat(DocGateRules.Categories.Select(category => $"- {category.Label}: {lines[category.Label]}\n"));
        return $"## Summary\n\nAdd the sample change.\n\n{DocGateRules.MatrixHeading}\n\n{matrix}";
    }

    private static Dictionary<string, string> Files(params (string Path, string Content)[] files)
    {
        return files.ToDictionary(file => file.Path, file => file.Content, StringComparer.Ordinal);
    }
}
