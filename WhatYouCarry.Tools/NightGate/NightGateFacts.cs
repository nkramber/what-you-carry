using System;
using WhatYouCarry.Tools.ReviewGate;

namespace WhatYouCarry.Tools.NightGate;

/// <summary>
/// One night record as the gate read it from one branch of the remote: the text, or the reason it is absent, and the
/// parsed record, or the reason the text is not a record.
/// </summary>
/// <param name="Branch">The branch of the remote that the gate read.</param>
/// <param name="Text">The text of the record, or null when the branch or the file is absent.</param>
/// <param name="AbsentReason">The reason the record is absent, or null when the text is present.</param>
/// <param name="Record">The parsed record, or null when the text is absent or is not a record.</param>
/// <param name="ParseError">The reason the text is not a record, or null when it is one or the text is absent.</param>
public sealed record NightRecordRead(string Branch, string? Text, string? AbsentReason, NightRecord? Record, string? ParseError);

/// <summary>
/// Everything the night gate rules need, read once from the remote branches and git. The rules do no I/O.
/// </summary>
/// <remarks>
/// <para>
/// The record of main comes from the branch <c>night-results</c> of the remote, through a fetch and a read of the
/// blob (D-273). The working tree of the checkout is never read, so a pull request cannot carry its own record
/// (PR #40 review P1-1). A remote with no such branch, or a branch with no such file, is an absent record with
/// the reason. Any other git failure is an error that names the command (T-2).
/// </para>
/// <para>
/// A night on the head branch of the PR writes its record to the branch <c>night-branch/&lt;head branch&gt;</c>, and
/// the gate reads it the same way (D-538). That record never replaces the record of main. The gate also reads the
/// effective head of the PR: the newest commit from the merge base that changes a path outside the skip set of
/// D-475 (D-534, D-547). A night at that commit, or at a later commit of the PR, ran the same code.
/// </para>
/// </remarks>
public sealed class NightGateFacts
{
    /// <summary>The name of the branch that holds the record of main (D-273).</summary>
    public const string RecordBranch = "night-results";

    /// <summary>The start of the name of the branch that holds the record of a night on another branch (D-538).</summary>
    public const string BranchRecordPrefix = "night-branch/";

    /// <summary>The name of the record file on each record branch (D-273).</summary>
    public const string RecordFile = "night.json";

    /// <summary>The record of main.</summary>
    public required NightRecordRead Main { get; init; }

    /// <summary>True when the commit of the record of main is on the base branch, false when it is not or the checkout lacks it, and null without a record.</summary>
    public required bool? CommitOnBase { get; init; }

    /// <summary>The base branch as a git revision, for example <c>origin/main</c>.</summary>
    public required string BaseRef { get; init; }

    /// <summary>The record of a night on the head branch of the PR.</summary>
    public required NightRecordRead Branch { get; init; }

    /// <summary>The effective head of the PR, or null when every commit of the PR changes documents alone (D-534).</summary>
    public required string? EffectiveHead { get; init; }

    /// <summary>
    /// True when the commit of the record of the head branch is the effective head of the PR, or a later commit of
    /// the PR, false when it is neither or the checkout lacks it, and null without a record (D-547).
    /// </summary>
    public required bool? BranchAtEffectiveHead { get; init; }

    /// <summary>The time of the evaluation, in UTC.</summary>
    public required DateTimeOffset Now { get; init; }

    /// <summary>Fetches the two record branches of the remote, reads each record from git, and asks git about the commits.</summary>
    /// <param name="root">The checkout.</param>
    /// <param name="remote">The name of the remote.</param>
    /// <param name="baseRef">The base branch as a git revision.</param>
    /// <param name="headBranch">The name of the head branch of the PR.</param>
    /// <param name="head">The head commit of the PR.</param>
    /// <param name="now">The time of the evaluation.</param>
    /// <exception cref="InvalidOperationException">A git command failed for a reason other than an absent branch. The message names the command and stderr.</exception>
    public static NightGateFacts Gather(string root, string remote, string baseRef, string headBranch, string head, DateTimeOffset now)
    {
        var git = new GitRepository(root);
        NightRecordRead main = ReadRecord(git, remote, RecordBranch);
        bool? commitOnBase = null;
        if (main.Record is not null)
        {
            commitOnBase = git.HasCommit(main.Record.Commit) && git.IsAncestor(main.Record.Commit, baseRef);
        }

        NightRecordRead branch = ReadRecord(git, remote, BranchRecordPrefix + headBranch);
        string mergeBase = git.MergeBase(baseRef, head);
        string? effectiveHead = git.NewestCommitOutside(mergeBase, head, ReviewGateRules.SkipPaths)?.Sha;
        bool? branchAtEffectiveHead = null;
        if (branch.Record is not null)
        {
            // Every commit after the effective head changes documents alone, so a night on any of them ran the same code.
            string commit = branch.Record.Commit;
            branchAtEffectiveHead = effectiveHead is not null && git.HasCommit(commit) && git.IsAncestor(effectiveHead, commit) && git.IsAncestor(commit, head);
        }

        return new NightGateFacts
        {
            Main = main,
            CommitOnBase = commitOnBase,
            BaseRef = baseRef,
            Branch = branch,
            EffectiveHead = effectiveHead,
            BranchAtEffectiveHead = branchAtEffectiveHead,
            Now = now,
        };
    }

    /// <summary>Fetches one record branch of the remote and reads its record from git, never from the working tree. The gate and the promotion both read records this way.</summary>
    public static NightRecordRead ReadRecord(GitRepository git, string remote, string branch)
    {
        if (!git.HasRemoteBranch(remote, branch))
        {
            return new NightRecordRead(branch, null, $"the remote '{remote}' has no branch {branch}", null, null);
        }

        git.Fetch(remote, branch);
        string? text = git.ReadFileOrNull("FETCH_HEAD", RecordFile);
        if (text is null)
        {
            return new NightRecordRead(branch, null, $"the branch {branch} of the remote '{remote}' holds no {RecordFile}", null, null);
        }

        NightRecord? record = NightRecordParser.TryParse(text, out string error);
        return new NightRecordRead(branch, text, null, record, record is null ? error : null);
    }
}
