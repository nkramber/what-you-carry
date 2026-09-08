using System.Collections.Generic;

namespace WhatYouCarry.Tools.ReviewGate;

/// <summary>
/// The input of the review-gate command. The workflow writes it as JSON (D-179, D-190).
/// Every field is required. An absent field is an error, never a default (T-2).
/// </summary>
public sealed class ReviewGateRequest
{
    /// <summary>The path of the git checkout that holds the PR head and the base branch.</summary>
    public required string RepositoryPath { get; init; }

    /// <summary>The GitHub PR number. It names the review file (D-101).</summary>
    public required int PullRequestNumber { get; init; }

    /// <summary>The PR head commit. The check run attaches to it (D-181).</summary>
    public required string HeadSha { get; init; }

    /// <summary>The base branch as a git revision, for example <c>origin/main</c>. The mode file is read from it (D-185).</summary>
    public required string BaseRef { get; init; }

    /// <summary>The label names on the PR at the time of the event.</summary>
    public required IReadOnlyList<string> Labels { get; init; }

    /// <summary>Every <c>labeled</c> event for the override label, from the PR timeline (D-190).</summary>
    public required IReadOnlyList<LabelEvent> OverrideLabelEvents { get; init; }
}

/// <summary>One <c>labeled</c> event from the PR timeline.</summary>
public sealed class LabelEvent
{
    /// <summary>The event time in ISO 8601 form.</summary>
    public required string CreatedAt { get; init; }

    /// <summary>The login of the account that added the label.</summary>
    public required string Actor { get; init; }
}
