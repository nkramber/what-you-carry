using System;
using System.Collections.Generic;
using System.Globalization;

namespace WhatYouCarry.Tools.ReviewGate;

/// <summary>The conclusion of the check run and its output text.</summary>
public sealed record ReviewGateResult(string Conclusion, string Title, string Summary)
{
    public const string Success = "success";
    public const string Failure = "failure";
    public const string Neutral = "neutral";
}

/// <summary>
/// The rules of the review gate (D-179, D-181, D-184, D-185, D-190). Pure: the facts go in, and a result comes out.
/// Every failure names the rule, the expected value, and the value found (T-2).
/// </summary>
public static class ReviewGateRules
{
    public const string CheckName = "review-gate";
    public const string ModeFilePath = ".github/review-gate-mode";
    public const string OverrideLabel = "review-override";
    public const string ApprovedVerdict = "Ready for owner merge";
    public const string ModeAdvisory = "advisory";
    public const string ModeEnforced = "enforced";

    /// <summary>A commit that changes only these paths is a metadata commit (D-184).</summary>
    public static readonly string[] MetadataPaths = ["docs/reviews/", "docs/session-handoff.md", "docs/session-handoff-archive.md"];

    /// <summary>The override label covers a PR whose every changed path starts with one of these (D-190).</summary>
    public static readonly string[] EligiblePaths = ["docs/", "CLAUDE.md", "AGENTS.md", ".claude/skills/"];

    public static string ReviewFilePath(int pullRequestNumber)
    {
        return $"docs/reviews/pr-{pullRequestNumber}.md";
    }

    public static ReviewGateResult Evaluate(ReviewGateFacts facts)
    {
        string? mode = ReadMode(facts.ModeText, out string modeError);
        if (mode is null)
        {
            return Fail("Mode file", $"'{ModeAdvisory}' or '{ModeEnforced}' in '{ModeFilePath}' on the base branch", modeError);
        }

        if (facts.HasOverrideLabel)
        {
            return EvaluateOverride(facts);
        }

        return EvaluateReview(facts, mode);
    }

    private static string? ReadMode(string? modeText, out string error)
    {
        if (modeText is null)
        {
            error = $"The file '{ModeFilePath}' is absent on the base branch.";
            return null;
        }

        string value = modeText.Trim();
        if (value.Length == 0)
        {
            error = $"The file '{ModeFilePath}' is empty.";
            return null;
        }

        if (value != ModeAdvisory && value != ModeEnforced)
        {
            error = $"The file '{ModeFilePath}' holds the unknown value '{value}'.";
            return null;
        }

        error = string.Empty;
        return value;
    }

    private static ReviewGateResult EvaluateOverride(ReviewGateFacts facts)
    {
        var codePaths = new List<string>();
        foreach (string path in facts.ChangedPaths)
        {
            if (!IsEligible(path))
            {
                codePaths.Add(path);
            }
        }

        if (codePaths.Count > 0)
        {
            return Fail(
                $"Override label '{OverrideLabel}': every changed path is in the eligible set ({string.Join(", ", EligiblePaths)})",
                "no changed path outside the eligible set",
                $"{codePaths.Count} path(s) outside the eligible set: {string.Join(", ", codePaths)}");
        }

        LabelEvent? labelEvent = facts.NewestOverrideLabelEvent;
        if (labelEvent is null)
        {
            return Fail(
                $"Override label '{OverrideLabel}': the PR timeline holds the labeled event",
                "one labeled event for the label",
                "no labeled event in the timeline");
        }

        DateTimeOffset labelTime = DateTimeOffset.Parse(labelEvent.CreatedAt, CultureInfo.InvariantCulture);
        string effectiveHeadText = facts.EffectiveHead?.Sha ?? "none (every commit in the range is a metadata commit)";
        if (facts.EffectiveHead is not null && facts.EffectiveHead.CommitTime > labelTime)
        {
            return Fail(
                $"Override label '{OverrideLabel}': no commit outside the metadata set is newer than the label event",
                $"effective head committed at or before {labelTime:O}",
                $"effective head {facts.EffectiveHead.Sha} committed at {facts.EffectiveHead.CommitTime:O}. Add the label again.");
        }

        return new ReviewGateResult(
            ReviewGateResult.Success,
            $"Override by label '{OverrideLabel}'",
            $"Label: {OverrideLabel}\nAdded by: {labelEvent.Actor} at {labelTime:O}\nEffective head: {effectiveHeadText}\nEvery changed path is in the eligible set (D-190).");
    }

    private static ReviewGateResult EvaluateReview(ReviewGateFacts facts, string mode)
    {
        string reviewFile = ReviewFilePath(facts.PullRequestNumber);
        if (facts.ReviewFileText is null)
        {
            string conclusion = mode == ModeAdvisory ? ReviewGateResult.Neutral : ReviewGateResult.Failure;
            return new ReviewGateResult(
                conclusion,
                "No review record",
                $"Rule: '{reviewFile}' exists on the PR head.\nExpected: the file on head {facts.HeadSha}.\nFound: no file. Mode: {mode}.");
        }

        ReviewRecord? record = ReviewRecord.TryParse(facts.ReviewFileText, out string parseError);
        if (record is null)
        {
            return Fail($"'{reviewFile}' holds a head and a verdict", "the '- Head:' line and the '## Verdict' section", parseError);
        }

        if (record.Verdict != ApprovedVerdict)
        {
            return Fail($"The verdict in '{reviewFile}' approves the PR", $"'{ApprovedVerdict}'", $"'{record.Verdict}'");
        }

        if (facts.EffectiveHead is null)
        {
            return Fail(
                "The head that the review records is the effective head",
                "one commit outside the metadata set",
                $"no commit outside the metadata set in the range. Every path is metadata, so the review path has nothing to approve. The '{OverrideLabel}' label covers this PR.");
        }

        if (!HeadMatches(record.RecordedHead, facts.EffectiveHead.Sha))
        {
            return Fail(
                "The head that the review records is the effective head (D-184)",
                $"'{facts.EffectiveHead.Sha}'",
                $"'{record.RecordedHead}'. A commit outside the metadata set came after the review.");
        }

        return new ReviewGateResult(
            ReviewGateResult.Success,
            "Approved review of the effective head",
            $"Review: {reviewFile}\nVerdict: {record.Verdict}\nEffective head: {facts.EffectiveHead.Sha}\nMode: {mode}.");
    }

    private static bool IsEligible(string path)
    {
        foreach (string eligible in EligiblePaths)
        {
            if (eligible.EndsWith('/') && path.StartsWith(eligible, StringComparison.Ordinal))
            {
                return true;
            }

            if (path == eligible)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>A short hash in the review file matches the full hash by prefix. Seven characters is the minimum.</summary>
    private static bool HeadMatches(string recorded, string effective)
    {
        if (recorded.Length < 7 || recorded.Length > effective.Length)
        {
            return false;
        }

        return effective.StartsWith(recorded, StringComparison.OrdinalIgnoreCase);
    }

    private static ReviewGateResult Fail(string rule, string expected, string found)
    {
        return new ReviewGateResult(ReviewGateResult.Failure, "Review gate failed", $"Rule: {rule}.\nExpected: {expected}.\nFound: {found}");
    }
}
