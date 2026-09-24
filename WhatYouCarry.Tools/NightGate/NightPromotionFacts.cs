using System;
using System.Collections.Generic;
using WhatYouCarry.Tools.CiSkip;
using WhatYouCarry.Tools.ReviewGate;

namespace WhatYouCarry.Tools.NightGate;

/// <summary>
/// Everything the promotion rules need, read once from the remote branches and git (D-555 to D-559). The rules do no
/// I/O.
/// </summary>
/// <remarks>
/// A push to main makes a merge commit. The record of the night on the head branch of the merged PR lives on the
/// branch <c>night-branch/&lt;head branch&gt;</c> (D-538). The facts hold that record, the record of main on
/// <c>night-results</c>, the paths outside the skip set of D-475 that differ between the two trees, and the place of
/// the record of main in the history of the merge commit.
/// </remarks>
public sealed class NightPromotionFacts
{
    /// <summary>The merge commit on main, as a full hash.</summary>
    public required string Merge { get; init; }

    /// <summary>The name of the head branch of the merged PR.</summary>
    public required string HeadBranch { get; init; }

    /// <summary>The record of main.</summary>
    public required NightRecordRead Main { get; init; }

    /// <summary>
    /// True when the commit of the record of main is an ancestor of the merge commit and not the merge commit itself,
    /// false when it is the merge commit, a later commit, a commit off its history, or a commit the checkout lacks,
    /// and null without a record (D-558).
    /// </summary>
    public required bool? MainBeforeMerge { get; init; }

    /// <summary>The record of the night on the head branch.</summary>
    public required NightRecordRead Branch { get; init; }

    /// <summary>True when the checkout holds the commit of the branch record, false when it does not, and null without a record.</summary>
    public required bool? BranchCommitKnown { get; init; }

    /// <summary>
    /// The paths outside the skip set of D-475 that differ between the tree of the branch night and the tree of the
    /// merge commit, or null when the checkout holds no commit of a branch record (D-555).
    /// </summary>
    public required IReadOnlyList<string>? CodePaths { get; init; }

    /// <summary>The time of the evaluation, in UTC.</summary>
    public required DateTimeOffset Now { get; init; }

    /// <summary>Fetches the two record branches of the remote, reads each record from git, and compares the trees.</summary>
    /// <param name="root">The checkout. It holds the history of main and the commits of the merged PR.</param>
    /// <param name="remote">The name of the remote.</param>
    /// <param name="merge">The merge commit on main.</param>
    /// <param name="headBranch">The name of the head branch of the merged PR.</param>
    /// <param name="now">The time of the evaluation.</param>
    /// <exception cref="InvalidOperationException">A git command failed for a reason other than an absent branch, or the checkout lacks the merge commit. The message names the command and stderr.</exception>
    public static NightPromotionFacts Gather(string root, string remote, string merge, string headBranch, DateTimeOffset now)
    {
        var git = new GitRepository(root);
        string mergeSha = git.Run(["rev-parse", "--verify", $"{merge}^{{commit}}"]).Trim();

        NightRecordRead main = NightGateFacts.ReadRecord(git, remote, NightGateFacts.RecordBranch);
        bool? mainBeforeMerge = null;
        if (main.Record is not null)
        {
            string commit = main.Record.Commit;
            mainBeforeMerge = commit != mergeSha && git.HasCommit(commit) && git.IsAncestor(commit, mergeSha);
        }

        NightRecordRead branch = NightGateFacts.ReadRecord(git, remote, NightGateFacts.BranchRecordPrefix + headBranch);
        bool? branchCommitKnown = null;
        List<string>? codePaths = null;
        if (branch.Record is not null)
        {
            branchCommitKnown = git.HasCommit(branch.Record.Commit);
            if (branchCommitKnown == true)
            {
                // A squash merge makes a new commit, so the trees decide, and not the history (D-555).
                codePaths = [];
                foreach (string path in git.ChangedPaths(branch.Record.Commit, mergeSha))
                {
                    if (!CiSkipRules.IsDocument(path))
                    {
                        codePaths.Add(path);
                    }
                }
            }
        }

        return new NightPromotionFacts
        {
            Merge = mergeSha,
            HeadBranch = headBranch,
            Main = main,
            MainBeforeMerge = mainBeforeMerge,
            Branch = branch,
            BranchCommitKnown = branchCommitKnown,
            CodePaths = codePaths,
            Now = now,
        };
    }
}
