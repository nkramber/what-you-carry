using System;
using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Tools.CiSkip;

namespace WhatYouCarry.Tools.ReviewGate;

/// <summary>The conclusion of the check run and its output text.</summary>
public sealed record ReviewGateResult(string Conclusion, string Title, string Summary)
{
    public const string Success = "success";
    public const string Failure = "failure";
    public const string Neutral = "neutral";
}

/// <summary>
/// The rules of the review gate (D-179, D-181, D-184, D-185, D-190, D-534, D-539, D-541, D-653). Pure: the facts go in, and a result comes out.
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

    /// <summary>A commit that changes only these paths is a metadata commit (D-184). It does not move the work head.</summary>
    public static readonly string[] MetadataPaths = ["docs/reviews/", "docs/session-handoff.md", "docs/session-handoff-archive.md"];

    /// <summary>
    /// A commit that changes only paths of the skip set of D-475 does not move the effective head (D-534). The override
    /// label covers a PR whose every changed path lies in this set (D-541). The CI skip owns the list, so the three rules
    /// never disagree on what a document is.
    /// </summary>
    public static IReadOnlyList<string> SkipPaths => CiSkipRules.DocumentPaths;

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
            if (!CiSkipRules.IsDocument(path))
            {
                codePaths.Add(path);
            }
        }

        if (codePaths.Count > 0)
        {
            return Fail(
                $"Override label '{OverrideLabel}': every changed path is in the skip set of D-475 ({string.Join(", ", SkipPaths)}) (D-541)",
                "no changed path outside the skip set",
                $"{codePaths.Count} path(s) outside the skip set: {string.Join(", ", codePaths)}");
        }

        LabelEvent? labelEvent = facts.NewestOverrideLabelEvent;
        if (labelEvent is null)
        {
            return Fail(
                $"Override label '{OverrideLabel}': the PR timeline holds the labeled event",
                "one labeled event for the label",
                "no labeled event in the timeline");
        }

        // The label reads the work head and not the effective head, so a documents commit after the label needs the
        // label again (D-190, D-539). The time of the work head is its push time: the earliest creation time of a
        // check suite that GitHub made for it. The committer date does not count, because the author can set it
        // (D-653).
        DateTimeOffset labelTime = DateTimeOffset.Parse(labelEvent.CreatedAt, CultureInfo.InvariantCulture);
        if (facts.WorkHead is null)
        {
            return OverrideSuccess(labelEvent.Actor, labelTime, "none (every commit in the range is a metadata commit)");
        }

        string workHead = facts.WorkHead.Sha;
        string timeRule = $"Override label '{OverrideLabel}': no commit outside the metadata set is newer than the label event, by the push time of the work head (D-539, D-653)";
        string expected = $"work head {workHead} pushed at or before the label event at {labelTime:O}";
        if (facts.WorkHeadCheckSuiteTimes is null)
        {
            return Fail(timeRule, expected, $"the check suites of work head {workHead} were not read, so its push time is unknown");
        }

        DateTimeOffset? pushTime = EarliestTime(facts.WorkHeadCheckSuiteTimes);
        if (pushTime is null)
        {
            return Fail(timeRule, expected, $"no check suite on work head {workHead}, so its push time is unknown. The label event is at {labelTime:O}");
        }

        if (pushTime.Value > labelTime)
        {
            return Fail(timeRule, expected, $"work head {workHead} pushed at {pushTime.Value:O}, after the label event at {labelTime:O}. Add the label again.");
        }

        return OverrideSuccess(labelEvent.Actor, labelTime, $"{workHead}, pushed at {pushTime.Value:O}");
    }

    private static ReviewGateResult OverrideSuccess(string actor, DateTimeOffset labelTime, string workHeadText)
    {
        return new ReviewGateResult(
            ReviewGateResult.Success,
            $"Override by label '{OverrideLabel}'",
            $"Label: {OverrideLabel}\nAdded by: {actor} at {labelTime:O}\nWork head: {workHeadText}\nEvery changed path is in the skip set of D-475 (D-190, D-541).");
    }

    /// <summary>The earliest of the times, or null when the list is empty.</summary>
    private static DateTimeOffset? EarliestTime(IReadOnlyList<DateTimeOffset> times)
    {
        DateTimeOffset? earliest = null;
        foreach (DateTimeOffset time in times)
        {
            if (earliest is null || time < earliest.Value)
            {
                earliest = time;
            }
        }

        return earliest;
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
            return Fail($"The verdict in '{reviewFile}' approves the PR", $"'{ApprovedVerdict}'", $"'{record.Verdict}'. {ReviewFileCommitLine(facts)}");
        }

        if (facts.EffectiveHead is null)
        {
            return Fail(
                "The head that the review records is the effective head (D-534)",
                "one commit outside the skip set of D-475",
                $"no commit outside the skip set in the range. Every path is a document, so the review path has nothing to approve. The '{OverrideLabel}' label covers this PR (D-540).");
        }

        if (!HeadMatches(record.RecordedHead, facts.EffectiveHead.Sha))
        {
            return Fail(
                "The head that the review records is the effective head (D-534)",
                $"'{facts.EffectiveHead.Sha}'",
                $"'{record.RecordedHead}'. A commit outside the skip set of D-475 came after the review.");
        }

        return new ReviewGateResult(
            ReviewGateResult.Success,
            "Approved review of the effective head",
            $"Review: {reviewFile}\nVerdict: {record.Verdict}\nEffective head: {facts.EffectiveHead.Sha}\n{ReviewFileCommitLine(facts)}\nMode: {mode}.");
    }

    /// <summary>
    /// Names the commit that last changed the review file. One shared identity cannot prove the reviewer (D-198),
    /// so the owner reads this line and recognizes a commit that the reviewer did not make.
    /// </summary>
    private static string ReviewFileCommitLine(ReviewGateFacts facts)
    {
        if (facts.ReviewFileCommit is null)
        {
            return "Review file last changed by: no commit";
        }

        return $"Review file last changed by: {facts.ReviewFileCommit.Sha} \"{facts.ReviewFileCommit.Subject}\"";
    }

    /// <summary>A short hash in the review file matches the full hash by prefix. Seven characters is the minimum.</summary>
    public static bool HeadMatches(string recorded, string effective)
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
