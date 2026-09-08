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

    /// <summary>The newest commit outside the metadata set, or null when every commit in the range is a metadata commit (D-184).</summary>
    public required CommitStamp? EffectiveHead { get; init; }

    /// <summary>The review file on the PR head, or null when it is absent (D-179).</summary>
    public required string? ReviewFileText { get; init; }

    /// <summary>The newest commit that changed the review file, or null when the file is absent. The output names it (D-198).</summary>
    public required CommitSubject? ReviewFileCommit { get; init; }

    /// <summary>Reads the facts for a request from its git checkout.</summary>
    public static ReviewGateFacts Gather(ReviewGateRequest request)
    {
        var git = new GitRepository(request.RepositoryPath);
        string mergeBase = git.MergeBase(request.BaseRef, request.HeadSha);
        return new ReviewGateFacts
        {
            PullRequestNumber = request.PullRequestNumber,
            HeadSha = request.HeadSha,
            ModeText = git.ReadFileOrNull(request.BaseRef, ReviewGateRules.ModeFilePath),
            HasOverrideLabel = request.Labels.Contains(ReviewGateRules.OverrideLabel),
            NewestOverrideLabelEvent = NewestEvent(request.OverrideLabelEvents),
            ChangedPaths = git.ChangedPaths(mergeBase, request.HeadSha),
            EffectiveHead = git.NewestCommitOutside(mergeBase, request.HeadSha, ReviewGateRules.MetadataPaths),
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
