using System;
using WhatYouCarry.Tools.ReviewGate;

namespace WhatYouCarry.Tools.NightGate;

/// <summary>
/// Everything the night gate rules need, read once from the remote branch and git. The rules do no I/O.
/// </summary>
/// <remarks>
/// The record comes from the branch <c>night-results</c> of the remote, through a fetch and a read of the blob
/// (D-273). The working tree of the checkout is never read, so a pull request cannot carry its own record
/// (PR #40 review P1-1). A remote with no such branch, or a branch with no such file, is an absent record with
/// the reason. Any other git failure is an error that names the command (T-2).
/// </remarks>
public sealed class NightGateFacts
{
    /// <summary>The name of the branch that holds the record (D-273).</summary>
    public const string RecordBranch = "night-results";

    /// <summary>The name of the record file on that branch (D-273).</summary>
    public const string RecordFile = "night.json";

    /// <summary>The text of the record, or null when the branch or the file is absent.</summary>
    public required string? RecordText { get; init; }

    /// <summary>The reason the record is absent, or null when the text is present.</summary>
    public required string? AbsentReason { get; init; }

    /// <summary>The parsed record, or null when the text is absent or is not a record.</summary>
    public required NightRecord? Record { get; init; }

    /// <summary>The reason the text is not a record, or null when it is one or the text is absent.</summary>
    public required string? ParseError { get; init; }

    /// <summary>True when the record commit is on the base branch, false when it is not or the checkout lacks it, and null without a record.</summary>
    public required bool? CommitOnBase { get; init; }

    /// <summary>The base branch as a git revision, for example <c>origin/main</c>.</summary>
    public required string BaseRef { get; init; }

    /// <summary>The time of the evaluation, in UTC.</summary>
    public required DateTimeOffset Now { get; init; }

    /// <summary>Fetches the record branch of the remote, reads the record from git, and asks git about its commit.</summary>
    /// <exception cref="InvalidOperationException">A git command failed for a reason other than an absent branch. The message names the command and stderr.</exception>
    public static NightGateFacts Gather(string root, string remote, string baseRef, DateTimeOffset now)
    {
        var git = new GitRepository(root);
        string? text = null;
        string? absentReason = null;
        if (!git.HasRemoteBranch(remote, RecordBranch))
        {
            absentReason = $"the remote '{remote}' has no branch {RecordBranch}";
        }
        else
        {
            git.Fetch(remote, RecordBranch);
            text = git.ReadFileOrNull("FETCH_HEAD", RecordFile);
            if (text is null)
            {
                absentReason = $"the branch {RecordBranch} of the remote '{remote}' holds no {RecordFile}";
            }
        }

        NightRecord? record = null;
        string? parseError = null;
        bool? commitOnBase = null;
        if (text is not null)
        {
            record = NightRecordParser.TryParse(text, out string error);
            if (record is null)
            {
                parseError = error;
            }
            else
            {
                commitOnBase = git.HasCommit(record.Commit) && git.IsAncestor(record.Commit, baseRef);
            }
        }

        return new NightGateFacts
        {
            RecordText = text,
            AbsentReason = absentReason,
            Record = record,
            ParseError = parseError,
            CommitOnBase = commitOnBase,
            BaseRef = baseRef,
            Now = now,
        };
    }
}
