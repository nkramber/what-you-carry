using System.Collections.Generic;
using System.IO;
using WhatYouCarry.Tools.ReviewGate;

namespace WhatYouCarry.Tools.DocGate;

/// <summary>
/// Everything the documentation gate rules need, read once from git and the PR description file. The rules do no I/O.
/// </summary>
/// <remarks>
/// The changed paths come from the merge base of the base branch and the head, as the review gate reads them
/// (D-179). The handoff text comes from the head commit, never from the working tree, so an uncommitted entry does
/// not pass the gate. The PR description is data from the event. It is not a file of the repository, so an edit of
/// the documents matrix moves no effective head (D-184).
/// </remarks>
public sealed class DocGateFacts
{
    /// <summary>The PR title.</summary>
    public required string Title { get; init; }

    /// <summary>The name of the PR head branch.</summary>
    public required string Branch { get; init; }

    /// <summary>The PR description. An empty description is an empty string.</summary>
    public required string Body { get; init; }

    /// <summary>Every path that the PR changes, from the merge base to the head, with forward slashes.</summary>
    public required IReadOnlyList<string> ChangedPaths { get; init; }

    /// <summary>The first session entry of the handoff at the head, or null when the file or the entry is absent.</summary>
    public required string? NewestHandoffEntry { get; init; }

    /// <summary>The full message of each commit from the base to the head, newest first. The attribution rule reads them (F-138).</summary>
    public required IReadOnlyList<CommitMessage> CommitMessages { get; init; }

    /// <summary>Reads the changed paths, the handoff, and the commit messages from git, and the description from its file.</summary>
    /// <exception cref="System.InvalidOperationException">A git command failed. The message names the command and stderr.</exception>
    /// <exception cref="FileNotFoundException">The description file is absent. The message names the path.</exception>
    public static DocGateFacts Gather(string root, string baseRef, string head, string bodyPath, string title, string branch)
    {
        if (!File.Exists(bodyPath))
        {
            throw new FileNotFoundException($"The PR description file '{bodyPath}' does not exist. The workflow writes it from the event before the gate runs.", bodyPath);
        }

        var git = new GitRepository(root);
        string mergeBase = git.MergeBase(baseRef, head);
        string? handoff = git.ReadFileOrNull(head, DocGateRules.HandoffPath);
        return new DocGateFacts
        {
            Title = title,
            Branch = branch,
            Body = File.ReadAllText(bodyPath),
            ChangedPaths = git.ChangedPaths(mergeBase, head),
            NewestHandoffEntry = handoff is null ? null : DocGateRules.NewestHandoffEntry(handoff),
            CommitMessages = git.CommitMessages(baseRef, head),
        };
    }
}
