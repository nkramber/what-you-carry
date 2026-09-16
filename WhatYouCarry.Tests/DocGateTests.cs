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

    private static DocGateFacts Facts(string body, IReadOnlyList<string> paths, string? handoffEntry = "", string title = Title, string branch = Branch)
    {
        return new DocGateFacts
        {
            Title = title,
            Branch = branch,
            Body = body,
            ChangedPaths = paths,
            NewestHandoffEntry = handoffEntry == string.Empty ? Entry(branch) : handoffEntry,
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
