using System;
using WhatYouCarry.Tools.CodexReview;
using WhatYouCarry.Tools.ReviewGate;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The pure rules of the review gate (D-179, D-181, D-185, D-190). No git in these tests.</summary>
[Trait("Category", DocumentsCategoryTests.DocumentsCategory)]
public sealed class ReviewGateRulesTests
{
    private const string Head = "0123456789abcdef0123456789abcdef01234567";

    [Fact]
    public void ReviewGateIsNeutralWithNoReviewFile()
    {
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(mode: "advisory", reviewFile: null));
        Assert.Equal(ReviewGateResult.Neutral, result.Conclusion);
        Assert.Contains("docs/reviews/pr-7.md", result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateFailsOnMissingFileWhenEnforced()
    {
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(mode: "enforced", reviewFile: null));
        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        AssertNamesRuleExpectedAndFound(result);
    }

    [Fact]
    public void ReviewGateFailsOnChangesRequired()
    {
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(mode: "advisory", reviewFile: ReviewFixture.Text(Head, "Changes required")));
        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains("'Changes required'", result.Summary, StringComparison.Ordinal);
        AssertNamesRuleExpectedAndFound(result);
    }

    [Fact]
    public void ReviewGateFailsOnBlocked()
    {
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(mode: "advisory", reviewFile: ReviewFixture.Text(Head, "Blocked")));
        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains("'Blocked'", result.Summary, StringComparison.Ordinal);
        AssertNamesRuleExpectedAndFound(result);
    }

    [Fact]
    public void ReviewGatePassesOnApproval()
    {
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(mode: "advisory", reviewFile: ReviewFixture.Text(Head[..7], "Ready for owner merge")));
        Assert.Equal(ReviewGateResult.Success, result.Conclusion);
        Assert.Contains(Head, result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateNamesTheCommitThatChangedTheReviewFile()
    {
        // D-198: the output names the commit that last changed the review file, on success and on a verdict failure.
        ReviewGateResult success = ReviewGateRules.Evaluate(Facts(mode: "advisory", reviewFile: ReviewFixture.Text(Head, "Ready for owner merge")));
        Assert.Contains("Review file last changed by: fedcba9876543210fedcba9876543210fedcba98 \"docs: review PR #7\"", success.Summary, StringComparison.Ordinal);

        ReviewGateResult failure = ReviewGateRules.Evaluate(Facts(mode: "advisory", reviewFile: ReviewFixture.Text(Head, "Blocked")));
        Assert.Contains("Review file last changed by: fedcba9876543210fedcba9876543210fedcba98", failure.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGatePassesOnApprovalInEnforcedMode()
    {
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(mode: "enforced", reviewFile: ReviewFixture.Text(Head, "Ready for owner merge")));
        Assert.Equal(ReviewGateResult.Success, result.Conclusion);
    }

    [Fact]
    public void ReviewGateFailsOnReviewFileWithoutVerdict()
    {
        string text = ReviewFixture.Text(Head, "Ready for owner merge").Replace("## Verdict", "## Outcome", StringComparison.Ordinal);
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(mode: "advisory", reviewFile: text));
        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains("## Verdict", result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateReadsTheVerdictSectionAndNotTheFindings()
    {
        // A finding that quotes "Blocked" before the verdict section must not change the verdict.
        string text = ReviewFixture.Text(Head, "Ready for owner merge")
            .Replace("## Findings\n", "## Findings\n\nP1-1 said Blocked at first, and the fix landed.\n", StringComparison.Ordinal);
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(mode: "advisory", reviewFile: text));
        Assert.Equal(ReviewGateResult.Success, result.Conclusion);
    }

    /// <summary>A section whose heading starts with the words of the Verdict heading is another section, so a history of earlier verdicts above it does not change the verdict (D-269, F-89).</summary>
    [Fact]
    public void ReviewGateReadsTheExactVerdictHeading()
    {
        string text = ReviewFixture.Text(Head, "Ready for owner merge")
            .Replace("## Verdict\n", "## Verdict history\n\nThe first pass gave the verdict Changes required for an earlier head.\n\n## Verdict\n", StringComparison.Ordinal);
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(mode: "enforced", reviewFile: text));
        Assert.Equal(ReviewGateResult.Success, result.Conclusion);

        string history = ReviewFixture.Text(Head, "Ready for owner merge")
            .Replace("## Verdict\n", "## Earlier verdicts\n\nChanges required at first.\n\n## Verdict\n", StringComparison.Ordinal);
        Assert.Equal(ReviewGateResult.Success, ReviewGateRules.Evaluate(Facts(mode: "enforced", reviewFile: history)).Conclusion);
    }

    /// <summary>A Verdict section that names two verdicts is an error that names both, and never the first one (D-269, F-89).</summary>
    [Fact]
    public void ReviewGateFailsOnTwoVerdictNames()
    {
        string text = ReviewFixture.Text(Head, "Ready for owner merge")
            .Replace("## Verdict\n", "## Verdict\n\nPrevious verdict: **Changes required** for an earlier head.\n", StringComparison.Ordinal);
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(mode: "enforced", reviewFile: text));
        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains("names 2 verdicts", result.Summary, StringComparison.Ordinal);
        Assert.Contains("Changes required, Ready for owner merge", result.Summary, StringComparison.Ordinal);
        Assert.Contains("D-269", result.Summary, StringComparison.Ordinal);

        string reversed = ReviewFixture.Text(Head, "Changes required")
            .Replace("One sentence of reason.", "The author asked for Ready for owner merge, and the evidence says no.", StringComparison.Ordinal);
        Assert.Equal(ReviewGateResult.Failure, ReviewGateRules.Evaluate(Facts(mode: "enforced", reviewFile: reversed)).Conclusion);
        Assert.Contains("names 2 verdicts", ReviewGateRules.Evaluate(Facts(mode: "enforced", reviewFile: reversed)).Summary, StringComparison.Ordinal);
    }

    /// <summary>
    /// F-116. A Verdict section whose first line does not start with the exact verdict name in bold fails, also
    /// when the one name that the section holds is the approving name. The old parse found the name inside other
    /// words and approved each of these texts (D-179, D-269).
    /// </summary>
    [Theory]
    [InlineData("**Not Ready for owner merge.** This verdict applies to head `{0}`.")]
    [InlineData("**Changes Required.** Ready for owner merge after P1-1.")]
    [InlineData("**ready for owner merge.** This verdict applies to head `{0}`.")]
    [InlineData("The verdict is **Ready for owner merge.** for head `{0}`.")]
    public void ReviewGateFailsOnAVerdictLineThatDoesNotStartWithTheName(string verdictLine)
    {
        string text = ReviewFixture.Text(Head, "Ready for owner merge")
            .Replace($"**Ready for owner merge.** This verdict applies to head `{Head}`.", string.Format(System.Globalization.CultureInfo.InvariantCulture, verdictLine, Head), StringComparison.Ordinal);
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(mode: "enforced", reviewFile: text));
        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains("## Verdict", result.Summary, StringComparison.Ordinal);
    }

    /// <summary>The boundary beside F-116: an exact bold name with more text after it on the same line approves.</summary>
    [Fact]
    public void ReviewGatePassesOnTheExactNameWithAReasonOnTheSameLine()
    {
        string text = ReviewFixture.Text(Head, "Ready for owner merge")
            .Replace("This verdict applies to head", "The fixes hold. This verdict applies to head", StringComparison.Ordinal);
        Assert.Equal(ReviewGateResult.Success, ReviewGateRules.Evaluate(Facts(mode: "enforced", reviewFile: text)).Conclusion);
    }

    /// <summary>
    /// F-116. A fenced example of an approving record above the real sections is not the record. The gate reads
    /// the head and the verdict outside each fence, so the real verdict decides.
    /// </summary>
    [Fact]
    public void ReviewGateSkipsAFencedExampleOfARecord()
    {
        string fence = "```\n- Head: `" + Head + "`\n\n## Verdict\n\n**Ready for owner merge.** This verdict applies to head `" + Head + "`.\n```\n\n";
        string text = ReviewFixture.Text("fedcba9", "Blocked").Replace("## Identity\n", fence + "## Identity\n", StringComparison.Ordinal);
        ReviewRecord? record = ReviewRecord.TryParse(text, out string error);
        Assert.True(record is not null, error);
        Assert.Equal("Blocked", record!.Verdict);
        Assert.Equal("fedcba9", record.RecordedHead);
        Assert.Equal(ReviewGateResult.Failure, ReviewGateRules.Evaluate(Facts(mode: "enforced", reviewFile: text)).Conclusion);
    }

    /// <summary>
    /// PR #104 P1-2. Only a matching fence closes a fenced block. Two tilde lines inside a backtick fence stay inside it,
    /// so the fake Identity and Verdict between them never reach the parse. The old parse toggled on any fence-like
    /// line, and it read the fake head and the fake approval.
    /// </summary>
    [Fact]
    public void ReviewGateKeepsATildeLineInsideABacktickFence()
    {
        string fake = "- Head: `" + Head + "`\n\n## Verdict\n\n**Ready for owner merge.** This verdict applies to head `" + Head + "`.\n";
        string fence = "```\n~~~\n" + fake + "~~~\n```\n\n";
        string text = ReviewFixture.Text("fedcba9", "Blocked").Replace("## Identity\n", fence + "## Identity\n", StringComparison.Ordinal);

        ReviewRecord? record = ReviewRecord.TryParse(text, out string error);

        Assert.True(record is not null, error);
        Assert.Equal("Blocked", record!.Verdict);
        Assert.Equal("fedcba9", record.RecordedHead);
    }

    /// <summary>The boundary beside PR #104 P1-2: a closing run shorter than the opening run stays inside, and a longer run of the same character closes the fence.</summary>
    [Fact]
    public void ReviewGateClosesAFenceOnAMatchingRunAlone()
    {
        string fake = "- Head: `" + Head + "`\n\n## Verdict\n\n**Ready for owner merge.** This verdict applies to head `" + Head + "`.\n";
        string shortClose = "````\n```\n" + fake + "````\n\n";
        string text = ReviewFixture.Text("fedcba9", "Blocked").Replace("## Identity\n", shortClose + "## Identity\n", StringComparison.Ordinal);
        ReviewRecord? record = ReviewRecord.TryParse(text, out string error);
        Assert.True(record is not null, error);
        Assert.Equal("Blocked", record!.Verdict);

        string longClose = "```\nnote\n`````\n\n";
        string closed = ReviewFixture.Text("fedcba9", "Blocked").Replace("## Identity\n", longClose + "## Identity\n", StringComparison.Ordinal);
        ReviewRecord? after = ReviewRecord.TryParse(closed, out string afterError);
        Assert.True(after is not null, afterError);
        Assert.Equal("fedcba9", after!.RecordedHead);
    }

    /// <summary>
    /// PR #104, the gitar finding on the fence indent. A fence opens after no more than three spaces, as in Markdown. A
    /// line with four spaces first is an indented code line, so the sections after it stay visible to the gate, as a
    /// reader sees them. Three spaces still open a fence.
    /// </summary>
    [Fact]
    public void ReviewGateReadsNoFenceAfterFourSpaces()
    {
        string fake = "- Head: `" + Head + "`\n\n## Verdict\n\n**Ready for owner merge.** This verdict applies to head `" + Head + "`.\n";
        string indented = "    ```\n\n";
        string text = ReviewFixture.Text("fedcba9", "Blocked").Replace("## Identity\n", indented + "## Identity\n", StringComparison.Ordinal);
        ReviewRecord? record = ReviewRecord.TryParse(text, out string error);
        Assert.True(record is not null, error);
        Assert.Equal("fedcba9", record!.RecordedHead);
        Assert.Equal("Blocked", record.Verdict);

        string threeSpaces = "   ```\n" + fake + "   ```\n\n";
        string fenced = ReviewFixture.Text("fedcba9", "Blocked").Replace("## Identity\n", threeSpaces + "## Identity\n", StringComparison.Ordinal);
        ReviewRecord? inside = ReviewRecord.TryParse(fenced, out string insideError);
        Assert.True(inside is not null, insideError);
        Assert.Equal("fedcba9", inside!.RecordedHead);
        Assert.Equal("Blocked", inside.Verdict);
    }

    /// <summary>
    /// PR #104 P1-3. A line of three backticks and an info string with a backtick opens no fence, as in Markdown. So the
    /// real Verdict after it stays visible, and the next line of three backticks opens the fence that hides the fake
    /// approval. The old parse opened the fence at the first line and read the fake approval. A backtick fence with a
    /// plain info string still opens.
    /// </summary>
    [Fact]
    public void ReviewGateOpensNoFenceOnABacktickInTheInfoString()
    {
        string fake = "## Verdict\n\n**Ready for owner merge.** This verdict applies to head `fedcba9`.\n";
        string text = ReviewFixture.Text("fedcba9", "Blocked").Replace("## Verdict\n", "```c`\n\n## Verdict\n", StringComparison.Ordinal) + "```\n" + fake + "```\n";
        ReviewRecord? record = ReviewRecord.TryParse(text, out string error);
        Assert.True(record is not null, error);
        Assert.Equal("Blocked", record!.Verdict);

        string plain = ReviewFixture.Text("fedcba9", "Blocked").Replace("## Identity\n", "```csharp\n" + fake + "```\n\n## Identity\n", StringComparison.Ordinal);
        ReviewRecord? fenced = ReviewRecord.TryParse(plain, out string fencedError);
        Assert.True(fenced is not null, fencedError);
        Assert.Equal("Blocked", fenced!.Verdict);
    }

    /// <summary>Every review record of the repository parses with one head and one verdict name, so a reviewer sees a second name locally before the push (D-269).</summary>
    [Fact]
    public void EveryRepositoryReviewRecordHoldsOneVerdict()
    {
        string root = System.IO.Path.Combine(RepositoryRoot.Find(), "docs", "reviews");
        int records = 0;
        foreach (string file in System.IO.Directory.EnumerateFiles(root, "pr-*.md"))
        {
            if (file.EndsWith("-response.md", StringComparison.Ordinal))
            {
                continue;
            }

            records++;
            ReviewRecord? record = ReviewRecord.TryParse(System.IO.File.ReadAllText(file), out string error);
            Assert.True(record is not null, $"{System.IO.Path.GetFileName(file)}: {error}");
            Assert.Contains(record!.Verdict, ReviewRecord.VerdictNames);
        }

        Assert.True(records >= 10, $"The repository holds {records} review records.");
    }

    /// <summary>
    /// The records of these PRs predate the format of <c>findings.md</c> that the findings parser reads (D-514), so
    /// their findings sections do not parse. <c>codex-review</c> judges the record of the PR under review alone, so the
    /// parser never reads them again.
    /// </summary>
    private static readonly string[] RecordsBeforeTheFindingsFormat = ["pr-10.md", "pr-40.md", "pr-43.md", "pr-84.md"];

    /// <summary>
    /// F-125: every review record of the repository with a findings section parses under the status rule. A record of
    /// <see cref="RecordsBeforeTheFindingsFormat"/> fails for a heading or a missing status line, and never for a status.
    /// </summary>
    [Fact]
    public void EveryRepositoryFindingsSectionParses()
    {
        string root = System.IO.Path.Combine(RepositoryRoot.Find(), "docs", "reviews");
        int sections = 0;
        var failures = new System.Collections.Generic.List<string>();
        foreach (string file in System.IO.Directory.EnumerateFiles(root, "pr-*.md"))
        {
            string name = System.IO.Path.GetFileName(file);
            string text = System.IO.File.ReadAllText(file);
            string[] lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            if (name.EndsWith("-response.md", StringComparison.Ordinal) || !Array.Exists(lines, static line => line.TrimEnd() == ReviewFindings.SectionHeading))
            {
                continue;
            }

            sections++;
            bool olderFormat = Array.IndexOf(RecordsBeforeTheFindingsFormat, name) >= 0;
            try
            {
                ReviewFindings.Parse(text);
                if (olderFormat)
                {
                    failures.Add($"{name} parses now, so it leaves the list of records before the findings format.");
                }
            }
            catch (FormatException exception)
            {
                if (!olderFormat || exception.Message.Contains("has the status", StringComparison.Ordinal))
                {
                    failures.Add($"{name}: {exception.Message}");
                }
            }
        }

        Assert.True(failures.Count == 0, string.Join("\n", failures));
        Assert.True(sections >= 10, $"The repository holds {sections} review records with a '{ReviewFindings.SectionHeading}' section.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  \n")]
    [InlineData("maybe")]
    public void ReviewGateFailsOnUnsetMode(string? modeText)
    {
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(mode: modeText, reviewFile: ReviewFixture.Text(Head, "Ready for owner merge")));
        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains(".github/review-gate-mode", result.Summary, StringComparison.Ordinal);
        AssertNamesRuleExpectedAndFound(result);
    }

    [Fact]
    public void ReviewGatePassesOnOverrideLabel()
    {
        ReviewGateFacts facts = Facts(mode: "advisory", reviewFile: null, overrideLabel: true,
            changedPaths: ["docs/design.md", "CLAUDE.md", "AGENTS.md", ".claude/skills/ste-writing/SKILL.md", "docs/reviews/pr-7.md"]);
        ReviewGateResult result = ReviewGateRules.Evaluate(facts);
        Assert.Equal(ReviewGateResult.Success, result.Conclusion);
        Assert.Contains("review-override", result.Summary, StringComparison.Ordinal);
        Assert.Contains("owner-login", result.Summary, StringComparison.Ordinal);
        Assert.Contains(Head, result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateOverrideLabelBeatsAReviewThatDoesNotApprove()
    {
        ReviewGateFacts facts = Facts(mode: "enforced", reviewFile: ReviewFixture.Text(Head, "Blocked"), overrideLabel: true,
            changedPaths: ["docs/design.md"]);
        Assert.Equal(ReviewGateResult.Success, ReviewGateRules.Evaluate(facts).Conclusion);
    }

    [Theory]
    [InlineData("WhatYouCarry.Game/README.md")]
    [InlineData(".github/pull_request_template.md")]
    public void ReviewGateFailsOnOverrideLabelWithADocumentOutsideTheSkipSet(string path)
    {
        // D-541: only the root README and LICENSE join the set. A Markdown file in another directory stays outside it.
        ReviewGateFacts facts = Facts(mode: "enforced", reviewFile: null, overrideLabel: true, changedPaths: [path]);
        ReviewGateResult result = ReviewGateRules.Evaluate(facts);
        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains(path, result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateFailsOnOverrideLabelWithCodePath()
    {
        ReviewGateFacts facts = Facts(mode: "advisory", reviewFile: null, overrideLabel: true,
            changedPaths: ["docs/design.md", "WhatYouCarry.Core/Core.cs", ".github/workflows/ci.yml"]);
        ReviewGateResult result = ReviewGateRules.Evaluate(facts);
        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains("WhatYouCarry.Core/Core.cs", result.Summary, StringComparison.Ordinal);
        Assert.Contains(".github/workflows/ci.yml", result.Summary, StringComparison.Ordinal);
        AssertNamesRuleExpectedAndFound(result);
    }

    [Fact]
    public void ReviewGateFailsOnOverrideLabelWithoutTimelineEvent()
    {
        ReviewGateFacts facts = Facts(mode: "advisory", reviewFile: null, overrideLabel: true, changedPaths: ["docs/design.md"], labelEvent: false);
        ReviewGateResult result = ReviewGateRules.Evaluate(facts);
        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        AssertNamesRuleExpectedAndFound(result);
    }

    [Fact]
    public void ReviewGateFailsOnOverrideLabelBeforeNewCommit()
    {
        ReviewGateFacts facts = Facts(mode: "advisory", reviewFile: null, overrideLabel: true, changedPaths: ["docs/design.md"],
            commitTime: DateTimeOffset.Parse("2026-09-07T12:00:01Z", System.Globalization.CultureInfo.InvariantCulture));
        ReviewGateResult result = ReviewGateRules.Evaluate(facts);
        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains("Add the label again", result.Summary, StringComparison.Ordinal);
        AssertNamesRuleExpectedAndFound(result);
    }

    [Fact]
    public void ReviewGateOverrideLabelReadsTheWorkHead()
    {
        // D-539: the label keeps the metadata set of D-190. A documents commit after the label has no effective head,
        // and the label still needs to come again.
        ReviewGateFacts facts = Facts(mode: "advisory", reviewFile: null, overrideLabel: true, changedPaths: ["docs/design.md"], documentsOnly: true,
            commitTime: DateTimeOffset.Parse("2026-09-07T12:00:01Z", System.Globalization.CultureInfo.InvariantCulture));
        ReviewGateResult result = ReviewGateRules.Evaluate(facts);
        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains("Add the label again", result.Summary, StringComparison.Ordinal);
        Assert.Contains("D-539", result.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateFailsOnAnApprovalOfADocumentsOnlyPullRequest()
    {
        // D-540: a PR of documents alone has no effective head, so the review path has nothing to approve. The
        // failure names the label.
        ReviewGateFacts facts = Facts(mode: "enforced", reviewFile: ReviewFixture.Text(Head, "Ready for owner merge"), changedPaths: ["docs/design.md"], documentsOnly: true);
        ReviewGateResult result = ReviewGateRules.Evaluate(facts);
        Assert.Equal(ReviewGateResult.Failure, result.Conclusion);
        Assert.Contains(ReviewGateRules.OverrideLabel, result.Summary, StringComparison.Ordinal);
        Assert.Contains("D-540", result.Summary, StringComparison.Ordinal);
        AssertNamesRuleExpectedAndFound(result);
    }

    [Theory]
    [InlineData("README.md")]
    [InlineData("LICENSE")]
    [InlineData("AGENTS.md")]
    [InlineData(".claude/skills/pr-review/SKILL.md")]
    public void ReviewGatePassesOnOverrideLabelForEachPathOfTheSkipSet(string path)
    {
        // D-541: the label covers each path of the skip set of D-475, the root README and LICENSE included.
        ReviewGateFacts facts = Facts(mode: "enforced", reviewFile: null, overrideLabel: true, changedPaths: [path], documentsOnly: true);
        Assert.Equal(ReviewGateResult.Success, ReviewGateRules.Evaluate(facts).Conclusion);
    }

    [Fact]
    public void TheSkipPathsAreTheDocumentsOfTheCiSkip()
    {
        // D-534: the review follows the skip set of D-475, and the CI skip owns that list.
        Assert.Equal(WhatYouCarry.Tools.CiSkip.CiSkipRules.DocumentPaths, ReviewGateRules.SkipPaths);
        Assert.Equal(["docs/", ".claude/skills/", "CLAUDE.md", "AGENTS.md", "README.md", "LICENSE"], ReviewGateRules.SkipPaths);
    }

    private static void AssertNamesRuleExpectedAndFound(ReviewGateResult result)
    {
        // T-2: each failure names the rule, the expected value, and the value found.
        Assert.Contains("Rule: ", result.Summary, StringComparison.Ordinal);
        Assert.Contains("Expected: ", result.Summary, StringComparison.Ordinal);
        Assert.Contains("Found: ", result.Summary, StringComparison.Ordinal);
    }

    private static ReviewGateFacts Facts(
        string? mode,
        string? reviewFile,
        bool overrideLabel = false,
        string[]? changedPaths = null,
        bool labelEvent = true,
        DateTimeOffset? commitTime = null,
        bool documentsOnly = false)
    {
        DateTimeOffset labelTime = DateTimeOffset.Parse("2026-09-07T12:00:00Z", System.Globalization.CultureInfo.InvariantCulture);
        return new ReviewGateFacts
        {
            PullRequestNumber = 7,
            HeadSha = Head,
            ModeText = mode,
            HasOverrideLabel = overrideLabel,
            NewestOverrideLabelEvent = labelEvent ? new LabelEvent { CreatedAt = labelTime.ToString("O"), Actor = "owner-login" } : null,
            ChangedPaths = changedPaths ?? ["WhatYouCarry.Core/Core.cs"],
            EffectiveHead = documentsOnly ? null : new CommitStamp(Head, commitTime ?? labelTime.AddHours(-1)),
            WorkHead = new CommitStamp(Head, commitTime ?? labelTime.AddHours(-1)),
            ReviewFileText = reviewFile,
            ReviewFileCommit = reviewFile is null ? null : new CommitSubject("fedcba9876543210fedcba9876543210fedcba98", "docs: review PR #7"),
        };
    }
}

/// <summary>Builds a review file in the skeleton of the pr-review skill.</summary>
internal static class ReviewFixture
{
    public static string Text(string head, string verdict)
    {
        return $"""
            # PR-7 review

            Date: 2026-09-07

            ## Identity

            - PR: 7
            - Target: `main`
            - Base: `abc1234`
            - Merge base: `abc1234`
            - Head: `{head}`
            - Branch: `feat/example`

            ## Provider gate

            The providers differ.

            ## Findings

            None.

            ## Verdict

            **{verdict}.** This verdict applies to head `{head}`.
            One sentence of reason.

            """;
    }
}
