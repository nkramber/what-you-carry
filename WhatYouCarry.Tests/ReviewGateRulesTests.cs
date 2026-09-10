using System;
using WhatYouCarry.Tools.ReviewGate;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The pure rules of the review gate (D-179, D-181, D-185, D-190). No git in these tests.</summary>
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
        DateTimeOffset? commitTime = null)
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
            EffectiveHead = new CommitStamp(Head, commitTime ?? labelTime.AddHours(-1)),
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
