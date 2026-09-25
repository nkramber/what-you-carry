using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WhatYouCarry.Tools;
using WhatYouCarry.Tools.SteCheck;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The fixture, the repository, the reference check, the session number check, and the CI job (PR-2 exit tests 2 to 9).</summary>
[Collection(ConsoleCollection.Name)]
[Trait("Category", DocumentsCategoryTests.DocumentsCategory)]
public sealed class SteCheckTests
{
    /// <summary>One violation per rule, and nothing else.</summary>
    private const string Fixture = """
        # Fixture

        ## Procedure

        1. Open the terminal, go to the checkout root, run the build command, then run the test command, and read the output.

        ## Notes

        The checker reads each file, each line, each sentence, and each word, and it reports the file, the line, the rule, the detail, and the text.
        The tool runs; it reports each finding.
        The tool doesn't run.
        The file was written by the tool.
        The owner should merge the PR.
        Running the tool takes time.
        """;

    private const string DecisionsFixture = """
        # Decisions

        | Id | Date | Topic | Decision | Effect |
        |---|---|---|---|---|
        | D-1 | 2026-09-06 | Old | The old answer. | Superseded by D-3 on 2026-09-07. |
        | D-2 | 2026-09-06 | Part | Two parts. | Revised in part by D-4 on 2026-09-07, the second part only. |
        | D-3 | 2026-09-07 | New | The new answer. | Supersedes D-1. |
        | D-4 | 2026-09-07 | Part two | The second part. | Revises D-2 in part. |
        """;

    [Fact]
    public void FixtureFindsEveryRule()
    {
        List<Finding> findings = SteChecker.Check("fixture.md", Fixture);
        string[] rules = findings.Select(f => f.Rule).OrderBy(r => r, StringComparer.Ordinal).ToArray();
        string[] expected =
        [
            SteRules.RuleHelperVerb, SteRules.RuleIngForm, SteRules.RulePassive, SteRules.RuleContraction,
            SteRules.RuleProceduralLength, SteRules.RuleDescriptiveLength, SteRules.RuleSemicolon,
        ];
        Assert.Equal(expected.OrderBy(r => r, StringComparer.Ordinal), rules);
        Assert.Equal(7, findings.Count);
    }

    [Fact]
    public void RepositoryDocumentsPass()
    {
        RepositoryCheckResult result = RepositoryCheck.Run(RepositoryRoot.Find());
        Assert.True(result.Files.Count >= 10, $"Only {result.Files.Count} document(s) found.");
        Assert.True(result.SteFindings.Count == 0, Report(result.SteFindings));
    }

    [Fact]
    public void RepositoryCitesNoSupersededDecision()
    {
        RepositoryCheckResult result = RepositoryCheck.Run(RepositoryRoot.Find());
        Assert.True(result.ReferenceFindings.Count == 0, Report(result.ReferenceFindings));
    }

    [Fact]
    public void RepositoryHandoffHasUniqueSessionNumbers()
    {
        RepositoryCheckResult result = RepositoryCheck.Run(RepositoryRoot.Find());
        Assert.True(result.SessionFindings.Count == 0, Report(result.SessionFindings));
    }

    [Fact]
    public void DatedRecordsAreExempt()
    {
        Assert.True(DocumentSet.IsExempt("docs/reviews/pr-1.md"));
        Assert.True(DocumentSet.IsExempt("docs/session-handoff.md"));
        Assert.True(DocumentSet.IsExempt("docs/session-handoff-archive.md"));
        Assert.True(DocumentSet.IsExempt("docs/archive/design-v1-2026-09-06.md"));
        Assert.False(DocumentSet.IsExempt("docs/design.md"));
        Assert.False(DocumentSet.IsExempt("docs/reviews.md"));
        Assert.False(DocumentSet.IsExempt("CLAUDE.md"));

        List<string> files = DocumentSet.Enumerate(RepositoryRoot.Find());
        Assert.Contains("docs/design.md", files);
        Assert.Contains(".claude/skills/ste-writing/SKILL.md", files);
        Assert.DoesNotContain("docs/session-handoff.md", files);
        Assert.DoesNotContain(files, path => path.StartsWith("docs/reviews/", StringComparison.Ordinal));
    }

    [Fact]
    public void ReferenceCheckReadsTheSupersededMarkerOnly()
    {
        Dictionary<string, string> superseded = ReferenceCheck.SupersededDecisions(DecisionsFixture);
        Assert.Equal(new Dictionary<string, string> { ["D-1"] = "D-3" }, superseded);
    }

    [Fact]
    public void ReferenceCheckFindsAStaleCitation()
    {
        Dictionary<string, string> superseded = ReferenceCheck.SupersededDecisions(DecisionsFixture);
        Finding finding = Assert.Single(ReferenceCheck.Check("docs/x.md", "Clean.\nThe answer is the old one (D-1).\n", superseded));
        Assert.Equal(ReferenceCheck.Rule, finding.Rule);
        Assert.Equal(2, finding.Line);
        Assert.Contains("cites D-1, which D-3 supersedes", finding.Detail, StringComparison.Ordinal);

        // A revision word, or the superseding decision on the same line, makes the citation self-consistent.
        Assert.Empty(ReferenceCheck.Check("docs/x.md", "D-3 supersedes D-1.\n", superseded));
        Assert.Empty(ReferenceCheck.Check("docs/x.md", "The old answer (D-1) was revised.\n", superseded));
        Assert.Empty(ReferenceCheck.Check("docs/x.md", "The answer (D-1, D-3).\n", superseded));
        // D-10 is not D-1.
        Assert.Empty(ReferenceCheck.Check("docs/x.md", "The answer (D-10).\n", superseded));
    }

    [Fact]
    public void ReferenceCheckReadsARevisionWordInItsOwnSentenceOnly()
    {
        // A paragraph is one line. A revision word in one sentence does not exempt a stale citation in another sentence.
        Dictionary<string, string> superseded = ReferenceCheck.SupersededDecisions(DecisionsFixture);
        Finding finding = Assert.Single(ReferenceCheck.Check("docs/x.md", "D-2 revises the count. The cap holds (D-1).\n", superseded));
        Assert.Equal(1, finding.Line);
        Assert.Contains("cites D-1, which D-3 supersedes", finding.Detail, StringComparison.Ordinal);

        // A revision word in the sentence of the citation, a parenthesis included, still exempts it.
        Assert.Empty(ReferenceCheck.Check("docs/x.md", "The cap holds. The old cap (D-1) was revised.\n", superseded));
        Assert.Empty(ReferenceCheck.Check("docs/x.md", "The cap holds (D-1, revised. The rest stands).\n", superseded));
    }

    [Fact]
    public void ReferenceCheckAllowsAPartialRevision()
    {
        // D-186: a decision marked "Revised in part by" stays citable.
        Dictionary<string, string> superseded = ReferenceCheck.SupersededDecisions(DecisionsFixture);
        Assert.Empty(ReferenceCheck.Check("docs/x.md", "The first part holds (D-2).\n", superseded));
    }

    [Fact]
    public void ReferenceCheckRejectsAMalformedRow()
    {
        string text = DecisionsFixture.Replace("| D-2 | 2026-09-06 | Part |", "| D-2 | 2026-09-06 |", StringComparison.Ordinal);
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => ReferenceCheck.SupersededDecisions(text));
        Assert.Contains("D-2", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SessionNumberCheckFindsADuplicate()
    {
        // D-187: two entries with the same number.
        string handoff = "# Session handoff\n\n## Session 12: 2026-09-07, Codex\n\ntext\n\n## Session 11: 2026-09-07, Codex\n\n## Session 11: 2026-09-07, Claude Code\n";
        Finding finding = Assert.Single(SessionNumberCheck.Check("docs/session-handoff.md", handoff));
        Assert.Equal(SessionNumberCheck.Rule, finding.Rule);
        Assert.Equal(9, finding.Line);
        Assert.Contains("session 11 also starts on line 7", finding.Detail, StringComparison.Ordinal);
        Assert.Empty(SessionNumberCheck.Check("docs/session-handoff.md", "## Session 12: a\n\n## Session 11: b\n"));
    }

    [Fact]
    public void SessionNumberCheckRejectsAFileWithNoEntry()
    {
        Assert.Throws<InvalidOperationException>(() => SessionNumberCheck.Check("docs/session-handoff.md", "# Session handoff\n"));
    }

    [Fact]
    public void SteCheckJobExists()
    {
        // PR-2 exit test 4: the ste-check job runs the command on every PR.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/ste-check.yml");
        Dictionary<string, string> runsOnByJob = WorkflowText.RunsOnByJob(workflow);
        Assert.Equal("ubuntu-latest", runsOnByJob["ste-check"]);
        Assert.Contains("\n  pull_request:\n", workflow, StringComparison.Ordinal);
        Assert.Contains("-- ste-check --root .", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void SteCheckCommandExitsOneOnAFindingAndZeroWhenClean()
    {
        string root = Path.Combine(Path.GetTempPath(), "wyc-ste-check-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "docs"));
        try
        {
            File.WriteAllText(Path.Combine(root, "docs", "decisions.md"), DecisionsFixture);
            File.WriteAllText(Path.Combine(root, "docs", "session-handoff.md"), "## Session 1: 2026-09-07, Codex\n");
            File.WriteAllText(Path.Combine(root, "docs", "design.md"), "The tool wrote the file.\n");
            Assert.Equal(0, Program.Main(["ste-check", "--root", root]));

            File.WriteAllText(Path.Combine(root, "docs", "design.md"), "The file was written by the tool.\n");
            Assert.Equal(1, Program.Main(["ste-check", "--root", root]));

            Assert.Equal(2, Program.Main(["ste-check"]));
            Assert.Equal(2, Program.Main(["ste-check", "--file", root]));
            // Review P2-3: a trailing argument, an extra option, and an option without a value are usage errors.
            Assert.Equal(2, Program.Main(["ste-check", "--root", root, "trailing-argument"]));
            Assert.Equal(2, Program.Main(["ste-check", "--root", root, "--unknown"]));
            Assert.Equal(2, Program.Main(["ste-check", "--root"]));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void SteCheckCommandFailsLoudlyWithoutTheRegisters()
    {
        string root = Path.Combine(Path.GetTempPath(), "wyc-ste-check-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            FileNotFoundException error = Assert.Throws<FileNotFoundException>(() => RepositoryCheck.Run(root));
            Assert.Contains("docs/decisions.md", error.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>
    /// D-386: the front matter takes rule 6.3 alone. The 27-word description of a skill turned the trunk red on
    /// 2026-09-17 (F-102), and a trigger list cannot pass the grammar rules. The body of the same file keeps every rule.
    /// </summary>
    [Fact]
    public void FrontMatterTakesTheSentenceLimitAlone()
    {
        const string longDescription = "description: Review a pull request and write the record, then answer each finding of the automated pass, "
            + "and use it for a repeat review after the author corrects the code.";
        string file = "---\nname: fixture\n" + longDescription + "\n---\n\nThe tool runs.\n";
        List<Finding> findings = SteChecker.Check("fixture.md", file);
        Finding single = Assert.Single(findings);
        Assert.Equal(SteRules.RuleDescriptiveLength, single.Rule);
        Assert.Equal(3, single.Line);

        // The grammar rules do not read the front matter: a passive form, a contraction, a helper verb, an -ing form,
        // and a semicolon all pass there.
        string quiet = "---\nname: fixture\ndescription: The file was written by the tool; it doesn't run.\n---\n\nThe tool runs.\n";
        Assert.Empty(SteChecker.Check("fixture.md", quiet));

        // The body of a file with front matter keeps every rule.
        string body = "---\nname: fixture\ndescription: A short line.\n---\n\nThe file was written by the tool.\n";
        Assert.Contains(SteChecker.Check("fixture.md", body), finding => finding.Rule == SteRules.RulePassive);
    }

    /// <summary>
    /// A file that opens with a thematic break and never closes it has no front matter. Without this rule the whole
    /// file reads as front matter, and no grammar rule sees it.
    /// </summary>
    [Fact]
    public void AnUnclosedOpeningBreakIsNotFrontMatter()
    {
        string file = "---\n\nThe file was written by the tool.\n";
        Assert.Contains(SteChecker.Check("fixture.md", file), finding => finding.Rule == SteRules.RulePassive);
    }

    [Fact]
    public void EnumerateReadsEachMarkdownExtensionInAnyCase()
    {
        // F-140: the old match read "*.md" alone, case-sensitive on the Linux CI job, and never read ".markdown".
        string root = TemporaryRoot(["docs/a.md", "docs/B.MD", "docs/c.markdown", "docs/D.Markdown", "docs/e.txt", "docs/f.mdx", "docs/g.md.bak"]);
        try
        {
            Assert.Equal(["docs/B.MD", "docs/D.Markdown", "docs/a.md", "docs/c.markdown"], DocumentSet.Enumerate(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void EnumerateSkipsTheWorktreesAtTheRootAndTheBuildDirectoriesAtAnyDepth()
    {
        // F-140: a worktree under .claude/worktrees/ is a copy of the checkout, and the old walk read each of its files.
        // The path skip holds at the root alone. A build directory name holds at any depth, as the skill states.
        string root = TemporaryRoot(
        [
            ".claude/worktrees/agent-1/docs/design.md", ".claude/skills/s/SKILL.md", "docs/worktrees/notes.md",
            "docs/.claude/worktrees/notes.md", "WhatYouCarry.Core/bin/Debug/readme.md", "WhatYouCarry.Core/obj/x.md",
            "WhatYouCarry.Game/.godot/y.md", "docs/bin/z.md",
        ]);
        try
        {
            Assert.Equal([".claude/skills/s/SKILL.md", "docs/.claude/worktrees/notes.md", "docs/worktrees/notes.md"], DocumentSet.Enumerate(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void TheSkillNamesTheFilesThatTheCheckerReads()
    {
        // F-140: the skill claimed every hand-written .md file, and the code skips four directory names at any depth.
        // The skill now names each extension and each skipped directory of the code.
        string checker = SkillSection("## The checker");
        foreach (string name in DocumentSet.MarkdownExtensions.Concat(DocumentSet.SkippedDirectoryNames))
        {
            Assert.Contains($"`{name}`", checker, StringComparison.Ordinal);
        }

        foreach (string path in DocumentSet.SkippedDirectoryPaths)
        {
            Assert.Contains($"`{path}/`", checker, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheSkillTableNamesEachWordThatTheRulesRead()
    {
        // F-140: the row of rule 3.4 named "is or are", and the code reads every auxiliary. The row of rule 4.2 named a
        // pronoun, and the code reads a wider list. The D-386 front matter rule had no row. SteRulesTests pins the words.
        string checker = SkillSection("## The checker");
        string helperRow = TableRow(checker, "STE 3.4");
        foreach (string auxiliary in SteRulesTests.Auxiliaries)
        {
            Assert.Matches($@"\b{auxiliary}\b[^.]*-ing form", helperRow);
        }

        string contractionRow = TableRow(checker, "STE 4.2");
        foreach (string word in SteRulesTests.ContractionWords)
        {
            Assert.Contains($"`{word}`", contractionRow, StringComparison.Ordinal);
        }

        Assert.Contains("rule 6.3 alone", TableRow(checker, "D-386"), StringComparison.Ordinal);
    }

    /// <summary>A new directory under the temp directory that holds an empty file at each relative path.</summary>
    private static string TemporaryRoot(string[] relativePaths)
    {
        string root = Path.Combine(Path.GetTempPath(), "wyc-ste-check-" + Guid.NewGuid().ToString("N"));
        foreach (string relativePath in relativePaths)
        {
            string path = Path.Combine(root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException($"'{path}' has no directory."));
            File.WriteAllText(path, "The tool runs.\n");
        }

        return root;
    }

    /// <summary>The text of the ste-writing skill from the heading to the next second-level heading.</summary>
    private static string SkillSection(string heading)
    {
        string skill = RepositoryRoot.ReadFile(".claude/skills/ste-writing/SKILL.md").Replace("\r\n", "\n", StringComparison.Ordinal);
        int start = skill.IndexOf($"\n{heading}\n", StringComparison.Ordinal);
        Assert.True(start >= 0, $"The ste-writing skill has no '{heading}' section.");
        int end = skill.IndexOf("\n## ", start + 1, StringComparison.Ordinal);
        return end < 0 ? skill[start..] : skill[start..end];
    }

    /// <summary>The one table row whose first cell is the rule id.</summary>
    private static string TableRow(string text, string ruleId)
    {
        return Assert.Single(text.Split('\n'), line => line.StartsWith($"| {ruleId} |", StringComparison.Ordinal));
    }

    private static string Report(List<Finding> findings)
    {
        return $"{findings.Count} finding(s):\n{string.Join('\n', findings)}";
    }
}
