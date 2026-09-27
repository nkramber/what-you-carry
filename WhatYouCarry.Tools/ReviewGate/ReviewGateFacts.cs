using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WhatYouCarry.Tools.ReviewGate;

/// <summary>Everything the rules need, read once from git and the request. The rules do no I/O.</summary>
public sealed class ReviewGateFacts
{
    public required int PullRequestNumber { get; init; }

    public required string HeadSha { get; init; }

    /// <summary>The text of the mode file on the base branch, or null when the file is absent (D-185).</summary>
    public required string? ModeText { get; init; }

    public required bool HasOverrideLabel { get; init; }

    /// <summary>The newest <c>labeled</c> event for the override label, or null when the timeline has none.</summary>
    public required LabelEvent? NewestOverrideLabelEvent { get; init; }

    /// <summary>Every path in the diff of the merge base and the head.</summary>
    public required IReadOnlyList<string> ChangedPaths { get; init; }

    /// <summary>
    /// The newest commit outside the skip set of D-475, or null when every commit in the range changes documents alone.
    /// The review path reads it (D-534).
    /// </summary>
    public required CommitStamp? EffectiveHead { get; init; }

    /// <summary>
    /// The newest commit outside the metadata set, or null when every commit in the range is a metadata commit. The
    /// override label reads it (D-184, D-190, D-539).
    /// </summary>
    public required CommitStamp? WorkHead { get; init; }

    /// <summary>
    /// The creation time of each check suite that GitHub made for the work head, from every page of the API. The
    /// earliest one is the push time of the work head (D-653). Gather reads it only for the override label, the one
    /// rule that reads it, so it is null when the label is absent or the work head is absent. An empty list means that
    /// the work head has no check suite.
    /// </summary>
    public required IReadOnlyList<DateTimeOffset>? WorkHeadCheckSuiteTimes { get; init; }

    /// <summary>The review file on the PR head, or null when it is absent (D-179).</summary>
    public required string? ReviewFileText { get; init; }

    /// <summary>The newest commit that changed the review file, or null when the file is absent. The output names it (D-198).</summary>
    public required CommitSubject? ReviewFileCommit { get; init; }

    /// <summary>
    /// Reads the facts for a request from its git checkout, and the check suites of the work head through the reader.
    /// The command passes <see cref="CheckSuites.ReadCreationTimes"/>, and a test passes a fixed list.
    /// </summary>
    /// <param name="request">The request that the workflow writes.</param>
    /// <param name="readCheckSuiteTimes">Returns the creation time of each check suite of the commit with the given hash (D-653).</param>
    public static ReviewGateFacts Gather(ReviewGateRequest request, Func<string, IReadOnlyList<DateTimeOffset>> readCheckSuiteTimes)
    {
        var git = new GitRepository(request.RepositoryPath);
        string mergeBase = git.MergeBase(request.BaseRef, request.HeadSha);
        bool hasOverrideLabel = request.Labels.Contains(ReviewGateRules.OverrideLabel);
        CommitStamp? workHead = git.NewestCommitOutside(mergeBase, request.HeadSha, ReviewGateRules.MetadataPaths);
        return new ReviewGateFacts
        {
            PullRequestNumber = request.PullRequestNumber,
            HeadSha = request.HeadSha,
            ModeText = git.ReadFileOrNull(request.BaseRef, ReviewGateRules.ModeFilePath),
            HasOverrideLabel = hasOverrideLabel,
            NewestOverrideLabelEvent = NewestEvent(request.OverrideLabelEvents),
            ChangedPaths = git.ChangedPaths(mergeBase, request.HeadSha),
            EffectiveHead = git.NewestCommitOutside(mergeBase, request.HeadSha, ReviewGateRules.SkipPaths),
            WorkHead = workHead,
            WorkHeadCheckSuiteTimes = hasOverrideLabel && workHead is not null ? readCheckSuiteTimes(workHead.Sha) : null,
            ReviewFileText = git.ReadFileOrNull(request.HeadSha, ReviewGateRules.ReviewFilePath(request.PullRequestNumber)),
            ReviewFileCommit = git.NewestCommitThatChanged(request.HeadSha, ReviewGateRules.ReviewFilePath(request.PullRequestNumber)),
        };
    }

    private static LabelEvent? NewestEvent(IReadOnlyList<LabelEvent> events)
    {
        LabelEvent? newest = null;
        DateTimeOffset newestTime = DateTimeOffset.MinValue;
        foreach (LabelEvent candidate in events)
        {
            DateTimeOffset time = DateTimeOffset.Parse(candidate.CreatedAt, CultureInfo.InvariantCulture);
            if (newest is null || time > newestTime)
            {
                newest = candidate;
                newestTime = time;
            }
        }

        return newest;
    }
}
